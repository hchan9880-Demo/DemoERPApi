using DemoERPApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;
using System.Text.RegularExpressions;
namespace DemoERPApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly string _connectionString;



    public CustomerController(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DemoERPConnection")
            ?? throw new InvalidOperationException("Missing connection string");
    }


    private bool IsValidEmail(string email)
        => Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$");


    private bool IsValidPhone(string phone)
        => Regex.IsMatch(phone,
            @"^\d{10}$");



    private string? GetCurrentUser()
    {
        return User.Claims
            .FirstOrDefault(c =>
                c.Type == "username" ||
                c.Type == ClaimTypes.Name ||
                c.Type == "name" ||
                c.Type == "preferred_username" ||
                c.Type == "sub")
            ?.Value;
    }


    private string? GetRole()
    {
        return User.FindFirst("role")?.Value
            ?? User.FindFirst(ClaimTypes.Role)?.Value;
    }

    private bool CanAccessCustomer(string crmId)
    {
        var currentUser = GetCurrentUser();
        var role = GetRole();


        Console.WriteLine("==============================");
        Console.WriteLine($"DEBUG CRM ID     : {crmId}");
        Console.WriteLine($"DEBUG USER       : {currentUser}");
        Console.WriteLine($"DEBUG ROLE       : {role}");
        Console.WriteLine($"DEBUG CONNECTION : {_connectionString}");
        Console.WriteLine("==============================");


        if (string.IsNullOrWhiteSpace(currentUser))
        {
            Console.WriteLine("DEBUG USER IS NULL");
            return false;
        }


        if (string.Equals(
            role,
            "Admin",
            StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("DEBUG ADMIN ACCESS");
            return true;
        }



        using var conn =
            new SqlConnection(_connectionString);

        conn.Open();



        const string sql = @"
        SELECT COUNT(*)
        FROM CustomerAccess
        WHERE CRMCustomerID = @crmId
        AND LOWER(Username) = LOWER(@username)";



        using var cmd =
            new SqlCommand(sql, conn);



        cmd.Parameters.AddWithValue(
            "@crmId",
            crmId);


        cmd.Parameters.AddWithValue(
            "@username",
            currentUser);



        var count =
            Convert.ToInt32(cmd.ExecuteScalar());



        Console.WriteLine(
            $"DEBUG ACCESS COUNT : {count}"
        );


        return count > 0;
    }








    // =====================================================
    // GET /api/Customer/{customerId}
    // =====================================================

    [HttpGet("{customerId}")]
    public IActionResult GetCustomer(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return BadRequest("CustomerID required");
        }


        var currentUser = GetCurrentUser();
        var role = GetRole();



        if (string.IsNullOrWhiteSpace(currentUser))
        {
            return Unauthorized();
        }



        if (string.IsNullOrWhiteSpace(role))
        {
            return Forbid();
        }



        using var conn =
            new SqlConnection(_connectionString);

        conn.Open();



        // =====================================================
        // STEP 1
        // Check customer exists first
        // Invalid customer = 404
        // =====================================================

        using var existsCmd =
            new SqlCommand(@"
            SELECT COUNT(*)
            FROM Customers
            WHERE CRMCustomerID = @id
            AND ISNULL(IsDeleted,0)=0",
                conn);



        existsCmd.Parameters.AddWithValue(
            "@id",
            customerId);



        int exists =
            Convert.ToInt32(
                existsCmd.ExecuteScalar());



        if (exists == 0)
        {
            return NotFound();
        }



        // =====================================================
        // STEP 2
        // Authorization
        // Admin can access everything
        // Customer can access only assigned CRM
        // =====================================================

        if (!role.Equals(
                "Admin",
                StringComparison.OrdinalIgnoreCase))
        {
            if (!CanAccessCustomer(customerId))
            {
                return Forbid();
            }
        }



        // =====================================================
        // STEP 3
        // Return customer data
        // =====================================================

        using var cmd =
            new SqlCommand(@"
            SELECT
                CRMCustomerID,
                FirstName,
                LastName,
                Email,
                Phone
            FROM Customers
            WHERE CRMCustomerID=@id
            AND ISNULL(IsDeleted,0)=0",
                conn);



        cmd.Parameters.AddWithValue(
            "@id",
            customerId);



        using var reader =
            cmd.ExecuteReader();



        if (!reader.Read())
        {
            return NotFound();
        }



        return Ok(new CustomerDto
        {
            CRMCustomerID =
                reader["CRMCustomerID"].ToString()!,


            FirstName =
                reader["FirstName"].ToString()!,


            LastName =
                reader["LastName"] == DBNull.Value
                ? null
                : reader["LastName"].ToString(),


            Email =
                reader["Email"].ToString()!,


            Phone =
                reader["Phone"].ToString()!
        });
    }










    // =====================================================
    // POST /api/Customer/sync
    // =====================================================


    [HttpPost("sync")]
    public IActionResult SyncCustomer(
        [FromBody] CustomerDto customer)
    {

        if (customer == null)
            return BadRequest("Payload required");


        if (string.IsNullOrWhiteSpace(customer.CRMCustomerID))
            return BadRequest("CustomerID required");


        if (!IsValidEmail(customer.Email))
            return BadRequest("Invalid email");


        if (!IsValidPhone(customer.Phone))
            return BadRequest("Phone must be 10 digits");



        var currentUser = GetCurrentUser();
        var role = GetRole();



        if (string.IsNullOrWhiteSpace(currentUser))
            return Forbid();



        if (role != "Admin" &&
            role != "Customer")
        {
            return Forbid();
        }




        using var conn =
            new SqlConnection(_connectionString);

        conn.Open();



        using var check =
            new SqlCommand(@"
                SELECT COUNT(*)
                FROM Customers
                WHERE CRMCustomerID=@id
                AND IsDeleted=0",
                conn);



        check.Parameters.AddWithValue("@id",
            customer.CRMCustomerID);



        if (Convert.ToInt32(check.ExecuteScalar()) > 0)
            return Conflict("Customer already exists");




        using var insert =
            new SqlCommand(@"
                INSERT INTO Customers
                (
                    CRMCustomerID,
                    FirstName,
                    LastName,
                    Email,
                    Phone,
                    IsDeleted
                )
                VALUES
                (
                    @id,
                    @first,
                    @last,
                    @email,
                    @phone,
                    0
                )",
                conn);



        insert.Parameters.AddWithValue("@id",
            customer.CRMCustomerID);

        insert.Parameters.AddWithValue("@first",
            customer.FirstName);

        insert.Parameters.AddWithValue("@last",
            customer.LastName ?? "");

        insert.Parameters.AddWithValue("@email",
            customer.Email);

        insert.Parameters.AddWithValue("@phone",
            customer.Phone);



        insert.ExecuteNonQuery();



        using var access =
            new SqlCommand(@"
                INSERT INTO CustomerAccess
                (
                    CRMCustomerID,
                    Username
                )
                VALUES
                (
                    @id,
                    @username
                )",
                conn);



        access.Parameters.AddWithValue("@id",
            customer.CRMCustomerID);

        access.Parameters.AddWithValue("@username",
            currentUser);



        access.ExecuteNonQuery();



        return Ok("Customer synced successfully");
    }







    // =====================================================
    // PUT /api/Customer/{customerId}
    // =====================================================
    [HttpPut("{customerId}")]
    public IActionResult UpdateCustomer(
      string customerId,
      [FromBody] CustomerDto customer)
    {
        var role = GetRole();

        if (string.IsNullOrWhiteSpace(role))
            return Forbid();


        // ==============================
        // Validate customer ID
        // ==============================

        if (string.IsNullOrWhiteSpace(customerId))
            return BadRequest("CustomerID is required");


        // ==============================
        // Validate request body (Email)
        // ==============================

        if (customer == null)
            return BadRequest("Customer data is required");


        if (!string.IsNullOrWhiteSpace(customer.Email))
        {
            if (!new System.ComponentModel.DataAnnotations
                .EmailAddressAttribute()
                .IsValid(customer.Email))
            {
                return BadRequest("Invalid email format");
            }
        }
        // ==============================
        // Validate request body (Phone)
        // ==============================

        if (string.IsNullOrWhiteSpace(customer.Phone))
        {
            return BadRequest("Phone is required");
        }

        if (!IsValidPhone(customer.Phone))
        {
            return BadRequest("Phone must be exactly 10 digits");
        }

        using var conn =
            new SqlConnection(_connectionString);

        conn.Open();



        // ==============================
        // Check customer exists
        // ==============================

        using var exists =
            new SqlCommand(@"
            SELECT COUNT(*)
            FROM Customers
            WHERE CRMCustomerID=@id
            AND IsDeleted=0",
                conn);


        exists.Parameters.AddWithValue(
            "@id",
            customerId);


        if (Convert.ToInt32(exists.ExecuteScalar()) == 0)
            return NotFound();



        // ==============================
        // Authorization
        // ==============================

        if (role != "Admin" &&
            !CanAccessCustomer(customerId))
        {
            return Forbid();
        }



        // ==============================
        // Update customer
        // ==============================

        using var update =
            new SqlCommand(@"
            UPDATE Customers
            SET
                FirstName = COALESCE(@first, FirstName),
                LastName  = COALESCE(@last, LastName),
                Email     = COALESCE(@email, Email),
                Phone     = COALESCE(@phone, Phone)

            WHERE CRMCustomerID=@id",
                conn);



        update.Parameters.AddWithValue(
            "@id",
            customerId);


        update.Parameters.AddWithValue(
            "@first",
            (object?)customer.FirstName ?? DBNull.Value);


        update.Parameters.AddWithValue(
            "@last",
            (object?)customer.LastName ?? DBNull.Value);


        update.Parameters.AddWithValue(
            "@email",
            (object?)customer.Email ?? DBNull.Value);


        update.Parameters.AddWithValue(
            "@phone",
            (object?)customer.Phone ?? DBNull.Value);



        var rows = update.ExecuteNonQuery();


        if (rows == 0)
            return NotFound();



        return Ok(new
        {
            message = "Customer updated successfully",
            customerId = customerId
        });
    }












    // =====================================================
    // DELETE /api/Customer/{customerId}
    // =====================================================

    [HttpDelete("{customerId}")]
    public IActionResult DeleteCustomer(string customerId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return BadRequest("CustomerID required");


            var role = GetRole();


            if (string.IsNullOrWhiteSpace(role))
                return Forbid();



            if (role != "Admin" &&
               !CanAccessCustomer(customerId))
            {
                return Forbid();
            }



            using var conn =
                new SqlConnection(_connectionString);

            conn.Open();



            using var exists =
                new SqlCommand(@"
                SELECT COUNT(*)
                FROM Customers
                WHERE CRMCustomerID = @id
                AND IsDeleted = 0",
                    conn);



            exists.Parameters.AddWithValue("@id", customerId);



            int count =
                Convert.ToInt32(exists.ExecuteScalar());



            if (count == 0)
            {
                return NotFound();
            }



            using var delete =
                new SqlCommand(@"
                UPDATE Customers
                SET IsDeleted = 1
                WHERE CRMCustomerID = @id",
                    conn);



            delete.Parameters.AddWithValue("@id", customerId);


            delete.ExecuteNonQuery();


            return Ok("Customer deleted successfully");

        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                ex.Message);
        }
    }




    // =====================================================
    // DebugCustomers /api/Customer/DebugCustomers
    // =====================================================
    // =====================================================
    // DebugCustomers
    // GET /api/Customer/debug/customers/list
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


        var result = new List<string>();


        using var reader = cmd.ExecuteReader();


        while (reader.Read())
        {
            result.Add(
                reader["CRMCustomerID"].ToString()!
            );
        }


        return Ok(result);
    }




    // =====================================================
    // DebugAccess 
    // =====================================================



    [AllowAnonymous]
    [HttpGet("debug/access")]
    public IActionResult DebugAccess()
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        var list = new List<object>();

        using var cmd = new SqlCommand(
            "SELECT Username, CRMCustomerID FROM CustomerAccess",
            conn);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new
            {
                Username = reader["Username"].ToString(),
                CRMCustomerID = reader["CRMCustomerID"].ToString()
            });
        }

        return Ok(new
        {
            Connection = _connectionString,
            Data = list
        });
    }

}