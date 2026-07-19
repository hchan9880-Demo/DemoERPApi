using DemoERPApi.Data;
using DemoERPApi.Exceptions;
using DemoERPApi.Interfaces;
using DemoERPApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;


namespace DemoERPApi.Controllers;


[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<CustomerController> _logger;
    private readonly IAuditService _auditService;
    private readonly ICustomerService _customerService;
    private readonly string _connectionString;


    public CustomerController(
        AppDbContext context,
        ILogger<CustomerController> logger,
        IConfiguration configuration,
        IAuditService auditService,
        ICustomerService customerService)
    {
        _context = context;
        _logger = logger;
        _auditService = auditService;
        _customerService = customerService;

        _connectionString =
            configuration.GetConnectionString(
                "DemoERPConnection");
    }




    // =====================================================
    // VALIDATION HELPERS
    // =====================================================

    private bool IsValidEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email)
            &&
            Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }



    private bool IsValidPhone(string phone)
    {
        return !string.IsNullOrWhiteSpace(phone)
            &&
            Regex.IsMatch(
                phone,
                @"^\d{10}$");
    }



    private bool ContainsSecurityPayload(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;


        var value =
            input.ToLower();


        return value.Contains("<script")
            || value.Contains("javascript:")
            || value.Contains("union select")
            || value.Contains("or 1=1");
    }




    // =====================================================
    // CURRENT USER FROM JWT
    // =====================================================

    private string GetCurrentUser()
    {
        return User.Claims
            .FirstOrDefault(c =>
                c.Type == "username"
                ||
                c.Type == ClaimTypes.Name
                ||
                c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value
            ?? "Unknown";
    }




    // =====================================================
    // ROLE FROM JWT
    // =====================================================

    private string? GetRole()
    {
        var claim =
            User.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Role
                ||
                string.Equals(
                    c.Type,
                    "role",
                    StringComparison.OrdinalIgnoreCase));


        return claim?.Value;
    }






    // =====================================================
    // CUSTOMER ACCESS CHECK
    //
    // FIX:
    // Old version checked Users.CustomerID
    // New version checks CustomerAccess table
    // =====================================================
    /*
    private bool CanAccessCustomer(string crmId)
    {
        var currentUser =
            GetCurrentUser();


        if (string.IsNullOrWhiteSpace(currentUser))
            return false;



        using var conn =
            new SqlConnection(_connectionString);


        conn.Open();


        const string sql = @"

SELECT COUNT(*)
FROM CustomerAccess
WHERE CRMCustomerID=@crmId
AND LOWER(Username)=LOWER(@username)

UNION ALL

SELECT COUNT(*)
FROM Users
WHERE CustomerID=@crmId
AND LOWER(Username)=LOWER(@username)
AND IsActive=1

";
        using var cmd =
            new SqlCommand(sql,
            conn);














        cmd.Parameters.AddWithValue(
            "@crmId",
            crmId);


        cmd.Parameters.AddWithValue(
            "@username",
            currentUser);






        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            if (reader.GetInt32(0) > 0)
                return true;
        }

        return false;

    }





    */

    private bool CanAccessCustomer(string crmId)
    {
        var currentUser = GetCurrentUser();

        Console.WriteLine("=================================");
        Console.WriteLine($"Current User = [{currentUser}]");
        Console.WriteLine($"Requested CRM = [{crmId}]");

        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        Console.WriteLine($"Database = {conn.Database}");

        using var cmd = new SqlCommand(@"
SELECT CRMCustomerID,
       Username
FROM CustomerAccess
ORDER BY CRMCustomerID
", conn);

        using var reader = cmd.ExecuteReader();

        Console.WriteLine("CustomerAccess table:");

        while (reader.Read())
        {
            Console.WriteLine(
                $"{reader["CRMCustomerID"]} -> {reader["Username"]}");
        }

        reader.Close();

        using var check = new SqlCommand(@"
SELECT COUNT(*)
FROM CustomerAccess
WHERE CRMCustomerID=@crmId
AND LOWER(Username)=LOWER(@username)
", conn);

        check.Parameters.AddWithValue("@crmId", crmId);
        check.Parameters.AddWithValue("@username", currentUser);

        var count = Convert.ToInt32(check.ExecuteScalar());

        Console.WriteLine($"Match Count = {count}");

        return count > 0;
    }










    // =====================================================
    // GET /api/Customer/{customerId}
    // =====================================================
    [HttpGet("{customerId}")] // Remove the ? here to force a specific ID
    [Authorize]
    public IActionResult GetCustomer(string customerId) // Remove nullable, as ID is required
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return NotFound("Customer ID not provided");
        }

        var currentUser = GetCurrentUser();
        var role = GetRole();

        if (string.IsNullOrWhiteSpace(role)) return Forbid();
        if (!HasValidRole()) return Forbid();
        if (string.IsNullOrWhiteSpace(currentUser)) return Unauthorized();

        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        // Check existence
        using var existsCmd = new SqlCommand(@"
            SELECT COUNT(*)
            FROM Customers
            WHERE CRMCustomerID=@id
            AND ISNULL(IsDeleted,0)=0", conn);

        existsCmd.Parameters.AddWithValue("@id", customerId);

        if (Convert.ToInt32(existsCmd.ExecuteScalar()) == 0)
        {
            _logger.LogWarning("Customer not found {CRMCustomerID}", customerId);
            return NotFound(); // Return 404 directly
        }

        // Authorization check
        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            if (!CanAccessCustomer(customerId))
            {
                _logger.LogWarning("Unauthorized access {CRMCustomerID} by {Username}", customerId, currentUser);
                return Forbid();
            }
        }

        // Retrieve data
        using var cmd = new SqlCommand(@"
            SELECT CRMCustomerID, FirstName, LastName, Email, Phone
            FROM Customers
            WHERE CRMCustomerID=@id
            AND ISNULL(IsDeleted,0)=0", conn);

        cmd.Parameters.AddWithValue("@id", customerId);

        using var reader = cmd.ExecuteReader();

        if (!reader.Read()) return NotFound(); // Return 404 directly

        return Ok(new CustomersDto
        {
            CRMCustomerID = reader["CRMCustomerID"].ToString()!,
            FirstName = reader["FirstName"].ToString()!,
            LastName = reader["LastName"] == DBNull.Value ? null : reader["LastName"].ToString(),
            Email = reader["Email"].ToString()!,
            Phone = reader["Phone"].ToString()!
        });
    }




    // =====================================================
    // GET /api/Customer/ (Fallback/Handler)
    // =====================================================
    [HttpGet]
    [Authorize] // Requires authentication but not Admin role
    public IActionResult HandleBaseCustomerRoute()
    {
        // If a user (like an Owner) hits /api/Customer/ 
        // without an ID, we return 404 instead of letting 
        // the Admin-only GetCustomers() method trigger a 403 Forbidden.
        return NotFound("Customer ID not provided");
    }

    // =====================================================
    // GET /api/Customer/ (Admin Collection)
    // =====================================================
    [HttpGet("list")] // Change this to /api/Customer/list
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCustomers()
    {
        var customers = await _customerService.GetCustomersAsync();
        return Ok(new ApiResponse<IEnumerable<Customers>>
        {
            Success = true,
            Data = customers,
            TraceId = HttpContext.TraceIdentifier
        });
    }
















    // =====================================================
    // POST /api/Customer/sync
    // =====================================================


    [HttpPost("sync")]
    public async Task<IActionResult> SyncCustomer([FromBody] CustomersDto customer)
    {
        if (customer == null)
            return BadRequest("Payload required");

        if (string.IsNullOrWhiteSpace(customer.CRMCustomerID))
            return BadRequest("CustomerID required");

        // Existing Phone Validation
        if (string.IsNullOrWhiteSpace(customer.Phone) || customer.Phone.Length < 10)
        {
            return BadRequest("Invalid phone format");
        }

        // --- ADDED SECURITY VALIDATION ---
        if (ContainsMaliciousContent(customer.FirstName) || ContainsMaliciousContent(customer.LastName))
        {
            return BadRequest("Invalid input content");
        }
        // ---------------------------------
        // --- ADD THIS VALIDATION ---
        if (!IsValidPhone(customer.Phone))
        {
            return BadRequest("Invalid phone format");
        }



        var currentUser = GetCurrentUser() ?? "Unknown";
        var role = GetRole() ?? "";

        if (!HasValidRole()) return Forbid();

        // --- ADD THIS AUTHORIZATION CHECK ---
        bool isQA = string.Equals(role, "QA", StringComparison.OrdinalIgnoreCase);
        if (isQA && !CanAccessCustomer(customer.CRMCustomerID))
        {
            return Forbid();
        }
        // ------------------------------------




        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // =====================================================
            // CHECK FOR EXISTING CUSTOMER
            // =====================================================
            using var existing = new SqlCommand(@"
            SELECT IsDeleted
            FROM Customers
            WHERE CRMCustomerID = @id
        ", conn);

            existing.Parameters.AddWithValue("@id", customer.CRMCustomerID);

            var result = await existing.ExecuteScalarAsync();

            if (result != null)
            {
                bool isDeleted = Convert.ToBoolean(result);

                if (!isDeleted)
                {
                    return Conflict("Customer already exists.");
                }

                // =====================================================
                // RESTORE SOFT-DELETED CUSTOMER
                // =====================================================
                using var restore = new SqlCommand(@"
                    UPDATE Customers 
                    SET FirstName = @first, 
                        LastName = @last, 
                        Email = @email, 
                        Phone = @phone, 
                        IsDeleted = 0, 
                        LastUpdated = GETDATE() 
                    WHERE CRMCustomerID = @id
                ", conn);

                restore.Parameters.AddWithValue("@id", customer.CRMCustomerID);
                restore.Parameters.AddWithValue("@first", customer.FirstName);
                restore.Parameters.AddWithValue("@last", (object?)customer.LastName ?? DBNull.Value);
                restore.Parameters.AddWithValue("@email", customer.Email);
                restore.Parameters.AddWithValue("@phone", customer.Phone);

                await restore.ExecuteNonQueryAsync();

                return Ok("Customer synced successfully"); // Returning OK satisfies SYNC_015
            }

            // =====================================================
            // INSERT NEW CUSTOMER
            // =====================================================
            using var insert = new SqlCommand(@"
            INSERT INTO Customers
            (
                CRMCustomerID,
                FirstName,
                LastName,
                Email,
                Phone,
                IsDeleted,
                LastUpdated
            )
            VALUES
            (
                @id,
                @first,
                @last,
                @email,
                @phone,
                0,
                GETDATE()
            )
        ", conn);

            insert.Parameters.AddWithValue("@id", customer.CRMCustomerID);
            insert.Parameters.AddWithValue("@first", customer.FirstName);
            insert.Parameters.AddWithValue("@last", (object?)customer.LastName ?? DBNull.Value);
            insert.Parameters.AddWithValue("@email", customer.Email);
            insert.Parameters.AddWithValue("@phone", customer.Phone);

            await insert.ExecuteNonQueryAsync();

            // =====================================================
            // AUDIT LOG
            // =====================================================
            try
            {
                await _auditService.LogCreateAsync(
                    "Customer",
                    customer.CRMCustomerID,
                    customer,
                    currentUser,
                    HttpContext.TraceIdentifier);
            }
            catch (Exception auditEx)
            {
                _logger.LogError(auditEx, "AUDIT LOG FAILED");
                return StatusCode(500, new
                {
                    error = "Audit logging failed",
                    detail = auditEx.ToString()
                });
            }

            // =====================================================
            // CUSTOMER ACCESS
            // =====================================================
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                using var access = new SqlCommand(@"
                INSERT INTO CustomerAccess
                (CRMCustomerID, Username)
                VALUES (@id, @username)
            ", conn);

                access.Parameters.AddWithValue("@id", customer.CRMCustomerID);
                access.Parameters.AddWithValue("@username", currentUser);

                await access.ExecuteNonQueryAsync();
            }

            return Ok("Customer synced successfully");
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL ERROR in SyncCustomer");

            return StatusCode(500, new
            {
                error = "SQL ERROR",
                detail = ex.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UNEXPECTED ERROR in SyncCustomer");

            return StatusCode(500, new
            {
                error = "UNEXPECTED ERROR",
                detail = ex.ToString()
            });
        }
    }











    private bool ContainsMaliciousContent(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;

        // Simple check for common XSS and SQL injection patterns
        var maliciousPatterns = new[] { "<script", "DROP TABLE", "--", "SELECT", "INSERT" };
        return maliciousPatterns.Any(pattern => input.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }


    // Helper method to keep code DRY
    private void AddCustomerParams(SqlCommand cmd, CustomersDto c)
    {
        cmd.Parameters.AddWithValue("@id", c.CRMCustomerID);
        cmd.Parameters.AddWithValue("@first", c.FirstName);
        cmd.Parameters.AddWithValue("@last", (object)c.LastName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@email", c.Email);
        cmd.Parameters.AddWithValue("@phone", c.Phone);
    }



    // =====================================================
    // PUT /api/Customer/{customerId}
    // =====================================================





    [HttpPut]
    public IActionResult HandleMissingId()
    {
        return NotFound("Customer ID not provided");
    }



    [HttpPut("{customerId?}")]
        public async Task<IActionResult> UpdateCustomer(
                string? customerId, // Change to nullable string
                [FromBody] CustomersDto customer)
        {
            // 1. Explicitly check if ID is missing. 
            // If it's missing, return NotFound immediately.
            if (string.IsNullOrWhiteSpace(customerId))
            {
                return NotFound("Customer ID not provided");
            }




            /*
            if (string.IsNullOrWhiteSpace(customerId))
            return BadRequest(
                "CustomerID required");
            */


        if (customerId == null)
        {
            /*  throw new ValidationException(
                  "Customer",
                  "Customer does not exist");*/

            return NotFound(
       "Customer does not exist");
        }

        if (string.IsNullOrWhiteSpace(customerId))
            return NotFound(
                "CustomerID required");




        if (!string.IsNullOrWhiteSpace(customer.Email)
            &&
            !IsValidEmail(customer.Email))
        {
            return BadRequest(
                "Invalid email format");
        }



        if (string.IsNullOrWhiteSpace(customer.Phone))
            return BadRequest(
                "Phone required");



        if (!IsValidPhone(customer.Phone))
            return BadRequest(
                "Phone must be exactly 10 digits");



        if (ContainsSecurityPayload(customer.FirstName)
            ||
            ContainsSecurityPayload(customer.LastName))
        {
            return BadRequest(
                "Malicious scripts detected.");
        }


        var currentUser = GetCurrentUser();
        var role = GetRole();

        if (string.IsNullOrWhiteSpace(role))
            return Forbid();

        if (!HasValidRole())
            return Forbid();

        if (string.IsNullOrWhiteSpace(currentUser))
            return Unauthorized();


        using var conn =
            new SqlConnection(_connectionString);


        conn.Open();



        using var exists =
            new SqlCommand(@"
            SELECT COUNT(*)
            FROM Customers
            WHERE CRMCustomerID=@id
            AND ISNULL(IsDeleted,0)=0
        ",
            conn);



        exists.Parameters.AddWithValue(
            "@id",
            customerId);



        if (Convert.ToInt32(
                exists.ExecuteScalar()) == 0)
        {
            //  return NotFound();
            throw new NotFoundException(
      "Customer not found");

        }




        if (!string.Equals(
                role,
                "Admin",
                StringComparison.OrdinalIgnoreCase))
        {
            if (!CanAccessCustomer(customerId))
                return Forbid();
        }



        // =====================================================
        // DUPLICATE CRM CUSTOMER ID CHECK
        // =====================================================

        if (!string.Equals(
                customerId,
                customer.CRMCustomerID,
                StringComparison.OrdinalIgnoreCase))
        {
            using var duplicate = new SqlCommand(@"
        SELECT COUNT(*)
        FROM Customers
        WHERE CRMCustomerID = @newId
        AND ISNULL(IsDeleted,0)=0
    ", conn);

            duplicate.Parameters.AddWithValue(
                "@newId",
                customer.CRMCustomerID);

            if (Convert.ToInt32(
                    duplicate.ExecuteScalar()) > 0)
            {
                return Conflict("Customer already exists.");
            }
        }



        // =====================================================
        // LOAD ORIGINAL CUSTOMER FOR AUDIT
        // =====================================================
        
        CustomersDto oldCustomer;


        using (var oldCmd = new SqlCommand(@"
SELECT
    CRMCustomerID,
    FirstName,
    LastName,
    Email,
    Phone
FROM Customers
WHERE CRMCustomerID=@id
",
        conn))
        {
            oldCmd.Parameters.AddWithValue(
                "@id",
                customerId);


            using var oldReader =
                oldCmd.ExecuteReader();


            if (!oldReader.Read())
                // return NotFound();
                throw new NotFoundException(
     "Customer not found");


            oldCustomer = new CustomersDto
            {
                CRMCustomerID =
                    oldReader["CRMCustomerID"].ToString()!,


                FirstName =
                    oldReader["FirstName"].ToString()!,


                LastName =
                    oldReader["LastName"] == DBNull.Value
                        ? null
                        : oldReader["LastName"].ToString(),


                Email =
                    oldReader["Email"].ToString()!,


                Phone =
                    oldReader["Phone"].ToString()!
            };
        }

        using var update =
            new SqlCommand(@"
                 UPDATE Customers
                SET
                    CRMCustomerID=@newId,
                    FirstName=@first,
                    LastName=@last,
                    Email=@email,
                    Phone=@phone,
                    LastUpdated=GETDATE()
                WHERE CRMCustomerID=@id
        ",
            conn);

        update.Parameters.AddWithValue(
            "@newId",
            customer.CRMCustomerID);

        update.Parameters.AddWithValue(
            "@id",
            customerId);


        update.Parameters.AddWithValue(
            "@first",
            customer.FirstName ?? "");


        update.Parameters.AddWithValue(
            "@last",
            customer.LastName ?? "");


        update.Parameters.AddWithValue(
            "@email",
            customer.Email ?? "");


        update.Parameters.AddWithValue(
            "@phone",
            customer.Phone);



        var rows =
            update.ExecuteNonQuery();

        // =====================================================
        // AUDIT LOG - UPDATE CUSTOMER
        // =====================================================

        await _auditService.LogUpdateAsync(
            "Customer",
            customerId,
            oldCustomer,
            customer,
            currentUser,
            HttpContext.TraceIdentifier);

        if (rows == 0)
        {
            _logger.LogWarning(
                "Customer update failed - not found {CRMCustomerID}",
                customerId);

            // return NotFound();
            throw new NotFoundException(
     "Customer not found");

        }


        _logger.LogInformation(
            "Customer updated {CRMCustomerID} by {Username}",
            customerId,
            currentUser);



        return Ok(
    new ApiResponse<CustomersDto>
    {
        Success = true,
        Data = customer,
        TraceId = HttpContext.TraceIdentifier
    });






        return Ok(new
        {
            message =
                "Customer updated successfully",

            customerId
        });
    }




















    // =====================================================
    // DELETE /api/Customer/{customerId}
    /*     
        Authenticate?
        No  -> 401

        Role present?
        No  -> 403

        Supported role?
        No  -> 403

        Customer exists?
        No  -> 404

        Ownership?
        No  -> 403

        Delete
     */
    // =====================================================
    [HttpDelete("{customerId}")] // Existing route
    [HttpDelete]               // Add this to handle /api/Customer/
    [Authorize]
    public async Task<IActionResult> DeleteCustomer(string? customerId)
    {
        // The framework will pass null/empty to customerId if no ID is provided in the route
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return NotFound("Customer ID not provided");
        }


        var currentUser =
            GetCurrentUser();


        var role =
            GetRole();



        if (string.IsNullOrWhiteSpace(currentUser))
            return Unauthorized();



        if (string.IsNullOrWhiteSpace(role))
            return Forbid();

        // =====================================================
        // VALIDATE SUPPORTED ROLE
        // =====================================================

        bool isAdmin =
            string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

        bool isQA =
            string.Equals(role, "QA", StringComparison.OrdinalIgnoreCase);

        bool isCustomer =
            string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase);

        if (!isAdmin && !isQA && !isCustomer)
        {
            return Forbid();
        }
        /*

        bool isAdmin =
            string.Equals(
                role,
                "Admin",
                StringComparison.OrdinalIgnoreCase);



        if (!isAdmin)
        {
            if (!CanAccessCustomer(customerId))
            {
                return Forbid();
            }
        }

*/

        using var conn =
            new SqlConnection(_connectionString);


        conn.Open();

        // =====================================================
        // EXISTING ACTIVE CUSTOMER CHECK
        // =====================================================

        using var exists =
            new SqlCommand(@"
        SELECT COUNT(*)
        FROM Customers
        WHERE CRMCustomerID = @id
          AND ISNULL(IsDeleted,0)=0
    ", conn);

        exists.Parameters.AddWithValue("@id", customerId);

        var found = Convert.ToInt32(exists.ExecuteScalar());

        if (found == 0)
        {
            // return NotFound();
            throw new NotFoundException(
    "Customer not found");

        }

        // =====================================================
        // AUTHORIZATION
        // =====================================================



        isAdmin =
               string.Equals(
                   role,
                   "Admin",
                   StringComparison.OrdinalIgnoreCase);

        if (!isAdmin)
        {
            if (!CanAccessCustomer(customerId))
            {
                return Forbid();
            }
        }



        isAdmin =
            string.Equals(
                role,
                "Admin",
                StringComparison.OrdinalIgnoreCase);

        if (!isAdmin)
        {
            if (!CanAccessCustomer(customerId))
            {
                return Forbid();
            }
        }


        // =====================================================
        // LOAD CUSTOMER BEFORE DELETE FOR AUDIT
        // =====================================================

        CustomersDto deletedCustomer;


        using (var cmd = new SqlCommand(@"
SELECT
    CRMCustomerID,
    FirstName,
    LastName,
    Email,
    Phone
FROM Customers
WHERE CRMCustomerID=@id
",
        conn))
        {
            cmd.Parameters.AddWithValue(
                "@id",
                customerId);


            using var reader =
                cmd.ExecuteReader();


            reader.Read();


            deletedCustomer = new CustomersDto
            {
                CRMCustomerID =
                    reader["CRMCustomerID"].ToString()!,


                FirstName =
                    reader["FirstName"].ToString()!,


                LastName =
                    reader["LastName"].ToString(),


                Email =
                    reader["Email"].ToString()!,


                Phone =
                    reader["Phone"].ToString()!
            };
        }

        // =====================================================
        // SOFT DELETE
        // =====================================================

        using var delete =
            new SqlCommand(@"
            UPDATE Customers
            SET
                IsDeleted = 1,
                LastUpdated = GETDATE()
            WHERE CRMCustomerID=@id
        ",
            conn);



        delete.Parameters.AddWithValue(
            "@id",
            customerId);



        var rows =
            delete.ExecuteNonQuery();

        // =====================================================
        // AUDIT LOG - DELETE CUSTOMER
        // =====================================================

        await _auditService.LogDeleteAsync(
            "Customer",
            customerId,
            deletedCustomer,
            currentUser,
            HttpContext.TraceIdentifier);

        if (rows == 0)
        {
            _logger.LogWarning(
                "Customer delete failed - not found {CRMCustomerID}",
                customerId);

            //    return NotFound();

            throw new NotFoundException(
    "Customer not found");


        }



        _logger.LogInformation(
         "Customer deleted {CRMCustomerID} by {Username}",
         customerId,
         currentUser);


        return Ok(
            "Customer deleted successfully");
    }





    /*
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCustomers() // Change to async
    {
        // Use the renamed method and await it
        var customers = await _customerService.GetCustomersAsync();

        return Ok(new ApiResponse<IEnumerable<Customers>>
        {
            Success = true,
            Data = customers,
            TraceId = HttpContext.TraceIdentifier
        });
    }
    */






    // =====================================================
    // DEBUG: LIST CUSTOMERS
    // =====================================================

    [AllowAnonymous]
    [HttpGet("debug/customers/list")]
    public IActionResult DebugCustomers()
    {
        using var conn =
            new SqlConnection(_connectionString);


        conn.Open();



        using var cmd =
            new SqlCommand(
                "SELECT CRMCustomerID FROM Customers",
                conn);



        var result =
            new List<string>();



        using var reader =
            cmd.ExecuteReader();



        while (reader.Read())
        {
            result.Add(
                reader["CRMCustomerID"]
                .ToString()!);
        }



        return Ok(result);
    }







    // =====================================================
    // DEBUG: CUSTOMER ACCESS
    // =====================================================

    [AllowAnonymous]
    [HttpGet("debug/access")]
    public IActionResult DebugAccess()
    {
        using var conn =
            new SqlConnection(_connectionString);


        conn.Open();



        var list =
            new List<object>();



        using var cmd =
            new SqlCommand(
                "SELECT Username, CRMCustomerID FROM CustomerAccess",
                conn);



        using var reader =
            cmd.ExecuteReader();



        while (reader.Read())
        {
            list.Add(new
            {
                Username =
                    reader["Username"]
                    .ToString(),

                CRMCustomerID =
                    reader["CRMCustomerID"]
                    .ToString()
            });
        }



        return Ok(list);
    }







    // =====================================================
    // DEBUG: WHO AM I
    // =====================================================

    [Authorize]
    [HttpGet("debug/whoami")]
    public IActionResult WhoAmI()
    {
        return Ok(new
        {
            Name =
                User.Identity?.Name,


            Authenticated =
                User.Identity?.IsAuthenticated,


            Claims =
                User.Claims.Select(c => new
                {
                    c.Type,
                    c.Value
                })
        });
    }
    private bool HasValidRole()
    {
        var role = GetRole();

        return
            string.Equals(role, "Admin",
                StringComparison.OrdinalIgnoreCase)
            ||
            string.Equals(role, "QA",
                StringComparison.OrdinalIgnoreCase)
            ||
            string.Equals(role, "Customer",
                StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateJwtToken(
    string username,
    string? role,
    string crmId)
    {
        var claims = new List<Claim>
    {
        new Claim(
            JwtRegisteredClaimNames.Sub,
            username),

        new Claim(
            "username",
            username),

        new Claim(
            "CRMCustomerID",
            crmId)
    };


        // Only add role when supplied
        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }


        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    "your-test-secret-key"));


        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);


        var token =
            new JwtSecurityToken(
                claims: claims,
                expires:
                    DateTime.UtcNow.AddMinutes(30),
                signingCredentials:
                    credentials);


        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    // =====================================================
    // WRITE SYNC LOG
    // =====================================================

    private void WriteSyncLog(
    string crmCustomerID,
    string operation,
    string status,
    string message,
    string username)
    {
        try
        {
            Console.WriteLine("Writing SyncLog...");

            var log = new SyncLogs
            {
                CRMCustomerID = crmCustomerID,
                Operation = operation,
                Status = status,
                Message = message,
                Username = username,
                RequestId = HttpContext.TraceIdentifier,
                   CreatedDate = DateTime.Now,
             //   CreatedDate = DateTime.UtcNow,
                ExecutionTimeMs = 0
            };


            _context.SyncLogs.Add(log);
            Console.WriteLine("==========================");
            Console.WriteLine(
                $"Connection = {_connectionString}");
            Console.WriteLine("==========================");
            var rows = _context.SaveChanges();


            Console.WriteLine(
                $"SyncLog saved rows={rows}");







        }
        catch (Exception ex)
        {
            Console.WriteLine("==============================");
            Console.WriteLine("SYNCLOG ERROR");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("==============================");

            throw;
        }
    }


}