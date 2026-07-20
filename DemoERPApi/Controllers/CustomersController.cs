using DemoERPApi.Data;
using DemoERPApi.Exceptions;
using DemoERPApi.Interfaces;
using DemoERPApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace DemoERPApi.Controllers;

/// <summary>
/// Controller for managing customer operations including CRUD operations,
/// synchronization, and access control.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerController"/> class.
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="configuration">Configuration for connection strings</param>
    /// <param name="auditService">Audit logging service</param>
    /// <param name="customerService">Customer business logic service</param>
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
        _connectionString = configuration.GetConnectionString("DemoERPConnection");
    }

    #region Validation Helpers

    /// <summary>
    /// Validates email format using regex pattern.
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>True if email format is valid</returns>
    private bool IsValidEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email)
            && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    /// <summary>
    /// Validates phone number format (exactly 10 digits).
    /// </summary>
    /// <param name="phone">Phone number to validate</param>
    /// <returns>True if phone format is valid</returns>
    private bool IsValidPhone(string phone)
    {
        return !string.IsNullOrWhiteSpace(phone)
            && Regex.IsMatch(phone, @"^\d{10}$");
    }

    /// <summary>
    /// Checks for common security payloads (XSS, SQL injection patterns).
    /// </summary>
    /// <param name="input">Input string to check</param>
    /// <returns>True if malicious content detected</returns>
    private bool ContainsSecurityPayload(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var value = input.ToLower();
        return value.Contains("<script")
            || value.Contains("javascript:")
            || value.Contains("union select")
            || value.Contains("or 1=1");
    }

    /// <summary>
    /// Checks for malicious content patterns including XSS and SQL injection.
    /// </summary>
    /// <param name="input">Input string to check</param>
    /// <returns>True if malicious content detected</returns>
    private bool ContainsMaliciousContent(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var maliciousPatterns = new[] { "<script", "DROP TABLE", "--", "SELECT", "INSERT" };
        return maliciousPatterns.Any(pattern =>
            input.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Validates if the current user has a valid role (Admin, QA, or Customer).
    /// </summary>
    /// <returns>True if user has a valid role</returns>
    private bool HasValidRole()
    {
        var role = GetRole();
        return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "QA", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region User Context Helpers

    /// <summary>
    /// Extracts the current username from the JWT token claims.
    /// </summary>
    /// <returns>Username or "Unknown" if not found</returns>
    private string GetCurrentUser()
    {
        return User.Claims
            .FirstOrDefault(c => c.Type == "username"
                || c.Type == ClaimTypes.Name
                || c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value
            ?? "Unknown";
    }

    /// <summary>
    /// Extracts the current user's role from the JWT token claims.
    /// </summary>
    /// <returns>Role or null if not found</returns>
    private string? GetRole()
    {
        var claim = User.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.Role
            || string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase));
        return claim?.Value;
    }

    #endregion

    #region Authorization Helpers

    /// <summary>
    /// Checks if the current user has access to a specific customer.
    /// Uses the CustomerAccess table for authorization.
    /// </summary>
    /// <param name="crmId">Customer CRM ID</param>
    /// <returns>True if user has access</returns>
    private bool CanAccessCustomer(string crmId)
    {
        var currentUser = GetCurrentUser();

        // Debug logging - preserved from original
        Console.WriteLine("=================================");
        Console.WriteLine($"Current User = [{currentUser}]");
        Console.WriteLine($"Requested CRM = [{crmId}]");

        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        Console.WriteLine($"Database = {conn.Database}");

        // Debug: Show all access entries
        using var debugCmd = new SqlCommand(@"
            SELECT CRMCustomerID, Username
            FROM CustomerAccess
            ORDER BY CRMCustomerID", conn);

        using var debugReader = debugCmd.ExecuteReader();
        Console.WriteLine("CustomerAccess table:");
        while (debugReader.Read())
        {
            Console.WriteLine($"{debugReader["CRMCustomerID"]} -> {debugReader["Username"]}");
        }
        debugReader.Close();

        // Check specific access
        using var check = new SqlCommand(@"
            SELECT COUNT(*)
            FROM CustomerAccess
            WHERE CRMCustomerID = @crmId
            AND LOWER(Username) = LOWER(@username)", conn);

        check.Parameters.AddWithValue("@crmId", crmId);
        check.Parameters.AddWithValue("@username", currentUser);

        var count = Convert.ToInt32(check.ExecuteScalar());
        Console.WriteLine($"Match Count = {count}");

        return count > 0;
    }

    #endregion

    #region GET Endpoints

    /// <summary>
    /// Retrieves a specific customer by ID.
    /// </summary>
    /// <param name="customerId">Customer CRM ID</param>
    /// <returns>Customer details or appropriate error response</returns>
    [HttpGet("{customerId}")]
    [Authorize]
    public IActionResult GetCustomer(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            return NotFound("Customer ID not provided");

        var currentUser = GetCurrentUser();
        var role = GetRole();

        if (string.IsNullOrWhiteSpace(role))
            return Forbid();
        if (!HasValidRole())
            return Forbid();
        if (string.IsNullOrWhiteSpace(currentUser))
            return Unauthorized();

        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        // Check if customer exists and is not deleted
        using var existsCmd = new SqlCommand(@"
            SELECT COUNT(*)
            FROM Customers
            WHERE CRMCustomerID = @id
            AND ISNULL(IsDeleted, 0) = 0", conn);

        existsCmd.Parameters.AddWithValue("@id", customerId);

        if (Convert.ToInt32(existsCmd.ExecuteScalar()) == 0)
        {
            _logger.LogWarning("Customer not found {CRMCustomerID}", customerId);
            return NotFound();
        }

        // Authorization check - Admins can access all, others need explicit access
        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            if (!CanAccessCustomer(customerId))
            {
                _logger.LogWarning("Unauthorized access {CRMCustomerID} by {Username}",
                    customerId, currentUser);
                return Forbid();
            }
        }

        // Retrieve customer data
        using var cmd = new SqlCommand(@"
            SELECT CRMCustomerID, FirstName, LastName, Email, Phone
            FROM Customers
            WHERE CRMCustomerID = @id
            AND ISNULL(IsDeleted, 0) = 0", conn);

        cmd.Parameters.AddWithValue("@id", customerId);

        using var reader = cmd.ExecuteReader();

        if (!reader.Read())
            return NotFound();

        return Ok(new CustomersDto
        {
            CRMCustomerID = reader["CRMCustomerID"].ToString()!,
            FirstName = reader["FirstName"].ToString()!,
            LastName = reader["LastName"] == DBNull.Value ? null : reader["LastName"].ToString(),
            Email = reader["Email"].ToString()!,
            Phone = reader["Phone"].ToString()!
        });
    }

    /// <summary>
    /// Handles base route without ID - returns 404 to prevent confusion.
    /// This prevents non-admin users from accidentally triggering the admin-only list endpoint.
    /// </summary>
    [HttpGet]
    [Authorize]
    public IActionResult HandleBaseCustomerRoute()
    {
        return NotFound("Customer ID not provided");
    }

    /// <summary>
    /// Retrieves all customers (Admin only).
    /// </summary>
    /// <returns>List of all customers</returns>
    [HttpGet("list")]
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

    #endregion

    #region POST Endpoints

    /// <summary>
    /// Syncs a customer from an external system.
    /// Creates new customer or restores soft-deleted customer.
    /// </summary>
    /// <param name="customer">Customer data to sync</param>
    /// <returns>Success or appropriate error response</returns>
    [HttpPost("sync")]
    public async Task<IActionResult> SyncCustomer([FromBody] CustomersDto customer)
    {
        if (customer == null)
            return BadRequest("Payload required");
        if (string.IsNullOrWhiteSpace(customer.CRMCustomerID))
            return BadRequest("CustomerID required");

        // Check if customer exists
        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CRMCustomerID == customer.CRMCustomerID);

        if (existingCustomer != null && existingCustomer.IsDeleted)
            return Conflict("Customer record is currently soft-deleted and cannot be synced directly.");

        if (existingCustomer != null)
            return Conflict("Customer already exists.");

        // Validations
        if (string.IsNullOrWhiteSpace(customer.Phone) || customer.Phone.Length < 10 || !IsValidPhone(customer.Phone))
            return BadRequest("Invalid phone format");

        if (ContainsMaliciousContent(customer.FirstName) || ContainsMaliciousContent(customer.LastName))
            return BadRequest("Invalid input content");

        if (!IsValidPhone(customer.Phone))
            return BadRequest("Invalid phone format");

        var currentUser = GetCurrentUser() ?? "Unknown";
        var role = GetRole() ?? "";

        if (!HasValidRole())
            return Forbid();

        // Authorization check for QA role
        bool isQA = string.Equals(role, "QA", StringComparison.OrdinalIgnoreCase);
        if (isQA && !CanAccessCustomer(customer.CRMCustomerID))
            return Forbid();

        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // Check for existing customer
            using var existing = new SqlCommand(@"
                SELECT IsDeleted
                FROM Customers
                WHERE CRMCustomerID = @id", conn);

            existing.Parameters.AddWithValue("@id", customer.CRMCustomerID);
            var result = await existing.ExecuteScalarAsync();

            if (result != null)
            {
                bool isDeleted = Convert.ToBoolean(result);

                if (!isDeleted)
                    return Conflict("Customer already exists.");

                // Restore soft-deleted customer
                using var restore = new SqlCommand(@"
                    UPDATE Customers 
                    SET FirstName = @first, 
                        LastName = @last, 
                        Email = @email, 
                        Phone = @phone, 
                        IsDeleted = 0, 
                        LastUpdated = GETDATE() 
                    WHERE CRMCustomerID = @id", conn);

                restore.Parameters.AddWithValue("@id", customer.CRMCustomerID);
                restore.Parameters.AddWithValue("@first", customer.FirstName);
                restore.Parameters.AddWithValue("@last", (object?)customer.LastName ?? DBNull.Value);
                restore.Parameters.AddWithValue("@email", customer.Email);
                restore.Parameters.AddWithValue("@phone", customer.Phone);

                await restore.ExecuteNonQueryAsync();
                return Ok("Customer synced successfully");
            }

            // Insert new customer
            using var insert = new SqlCommand(@"
                INSERT INTO Customers
                (CRMCustomerID, FirstName, LastName, Email, Phone, IsDeleted, LastUpdated)
                VALUES
                (@id, @first, @last, @email, @phone, 0, GETDATE())", conn);

            insert.Parameters.AddWithValue("@id", customer.CRMCustomerID);
            insert.Parameters.AddWithValue("@first", customer.FirstName);
            insert.Parameters.AddWithValue("@last", (object?)customer.LastName ?? DBNull.Value);
            insert.Parameters.AddWithValue("@email", customer.Email);
            insert.Parameters.AddWithValue("@phone", customer.Phone);

            await insert.ExecuteNonQueryAsync();

            // Audit logging
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

            // Grant customer access for non-admin users
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                using var access = new SqlCommand(@"
                    INSERT INTO CustomerAccess
                    (CRMCustomerID, Username)
                    VALUES (@id, @username)", conn);

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

    #endregion

    #region PUT Endpoints

    /// <summary>
    /// Handles missing ID for PUT requests - returns 404.
    /// </summary>
    [HttpPut]
    public IActionResult HandleMissingId()
    {
        return NotFound("Customer ID not provided");
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    /// <param name="customerId">Customer CRM ID to update</param>
    /// <param name="customer">Updated customer data</param>
    /// <returns>Updated customer or appropriate error response</returns>
    [HttpPut("{customerId?}")]
    public async Task<IActionResult> UpdateCustomer(
        string? customerId,
        [FromBody] CustomersDto customer)
    {
        // Validate ID
        if (string.IsNullOrWhiteSpace(customerId))
            return NotFound("Customer ID not provided");

        // Validate customer data
        if (!string.IsNullOrWhiteSpace(customer.Email) && !IsValidEmail(customer.Email))
            return BadRequest("Invalid email format");

        if (string.IsNullOrWhiteSpace(customer.Phone))
            return BadRequest("Phone required");

        if (!IsValidPhone(customer.Phone))
            return BadRequest("Phone must be exactly 10 digits");

        if (ContainsSecurityPayload(customer.FirstName) || ContainsSecurityPayload(customer.LastName))
            return BadRequest("Malicious scripts detected.");

        var currentUser = GetCurrentUser();
        var role = GetRole();

        if (string.IsNullOrWhiteSpace(role))
            return Forbid();
        if (!HasValidRole())
            return Forbid();
        if (string.IsNullOrWhiteSpace(currentUser))
            return Unauthorized();

        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        // Check if customer exists
        using var exists = new SqlCommand(@"
            SELECT COUNT(*)
            FROM Customers
            WHERE CRMCustomerID = @id
            AND ISNULL(IsDeleted, 0) = 0", conn);

        exists.Parameters.AddWithValue("@id", customerId);

        if (Convert.ToInt32(exists.ExecuteScalar()) == 0)
        {
            throw new NotFoundException("Customer not found");
        }

        // Authorization check
        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            if (!CanAccessCustomer(customerId))
                return Forbid();
        }

        // Check for duplicate CRM ID
        if (!string.Equals(customerId, customer.CRMCustomerID, StringComparison.OrdinalIgnoreCase))
        {
            using var duplicate = new SqlCommand(@"
                SELECT COUNT(*)
                FROM Customers
                WHERE CRMCustomerID = @newId
                AND ISNULL(IsDeleted, 0) = 0", conn);

            duplicate.Parameters.AddWithValue("@newId", customer.CRMCustomerID);

            if (Convert.ToInt32(duplicate.ExecuteScalar()) > 0)
                return Conflict("Customer already exists.");
        }

        // Load original customer for audit
        CustomersDto oldCustomer;

        using (var oldCmd = new SqlCommand(@"
            SELECT CRMCustomerID, FirstName, LastName, Email, Phone
            FROM Customers
            WHERE CRMCustomerID = @id", conn))
        {
            oldCmd.Parameters.AddWithValue("@id", customerId);

            using var oldReader = oldCmd.ExecuteReader();

            if (!oldReader.Read())
                throw new NotFoundException("Customer not found");

            oldCustomer = new CustomersDto
            {
                CRMCustomerID = oldReader["CRMCustomerID"].ToString()!,
                FirstName = oldReader["FirstName"].ToString()!,
                LastName = oldReader["LastName"] == DBNull.Value ? null : oldReader["LastName"].ToString(),
                Email = oldReader["Email"].ToString()!,
                Phone = oldReader["Phone"].ToString()!
            };
        }

        // Update customer
        using var update = new SqlCommand(@"
            UPDATE Customers
            SET CRMCustomerID = @newId,
                FirstName = @first,
                LastName = @last,
                Email = @email,
                Phone = @phone,
                LastUpdated = GETDATE()
            WHERE CRMCustomerID = @id", conn);

        update.Parameters.AddWithValue("@newId", customer.CRMCustomerID);
        update.Parameters.AddWithValue("@id", customerId);
        update.Parameters.AddWithValue("@first", customer.FirstName ?? "");
        update.Parameters.AddWithValue("@last", customer.LastName ?? "");
        update.Parameters.AddWithValue("@email", customer.Email ?? "");
        update.Parameters.AddWithValue("@phone", customer.Phone);

        var rows = update.ExecuteNonQuery();

        // Audit logging
        await _auditService.LogUpdateAsync(
            "Customer",
            customerId,
            oldCustomer,
            customer,
            currentUser,
            HttpContext.TraceIdentifier);

        if (rows == 0)
        {
            _logger.LogWarning("Customer update failed - not found {CRMCustomerID}", customerId);
            throw new NotFoundException("Customer not found");
        }

        _logger.LogInformation("Customer updated {CRMCustomerID} by {Username}", customerId, currentUser);

        return Ok(new ApiResponse<CustomersDto>
        {
            Success = true,
            Data = customer,
            TraceId = HttpContext.TraceIdentifier
        });
    }

    #endregion

    #region DELETE Endpoints

    /// <summary>
    /// Soft-deletes a customer.
    /// </summary>
    /// <param name="customerId">Customer CRM ID to delete</param>
    /// <returns>Success or appropriate error response</returns>
    [HttpDelete("{customerId}")]
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteCustomer(string? customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            return NotFound("Customer ID not provided");

        var currentUser = GetCurrentUser();
        var role = GetRole();

        if (string.IsNullOrWhiteSpace(currentUser))
            return Unauthorized();
        if (string.IsNullOrWhiteSpace(role))
            return Forbid();

        // Validate supported roles
        bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        bool isQA = string.Equals(role, "QA", StringComparison.OrdinalIgnoreCase);
        bool isCustomer = string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase);

        if (!isAdmin && !isQA && !isCustomer)
            return Forbid();

        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        // Check if customer exists and is active
        using var exists = new SqlCommand(@"
            SELECT COUNT(*)
            FROM Customers
            WHERE CRMCustomerID = @id
            AND ISNULL(IsDeleted, 0) = 0", conn);

        exists.Parameters.AddWithValue("@id", customerId);
        var found = Convert.ToInt32(exists.ExecuteScalar());

        if (found == 0)
            throw new NotFoundException("Customer not found");

        // Authorization check - non-admin users must have access
        if (!isAdmin)
        {
            if (!CanAccessCustomer(customerId))
                return Forbid();
        }

        // Load customer data for audit
        CustomersDto deletedCustomer;

        using (var cmd = new SqlCommand(@"
            SELECT CRMCustomerID, FirstName, LastName, Email, Phone
            FROM Customers
            WHERE CRMCustomerID = @id", conn))
        {
            cmd.Parameters.AddWithValue("@id", customerId);

            using var reader = cmd.ExecuteReader();
            reader.Read();

            deletedCustomer = new CustomersDto
            {
                CRMCustomerID = reader["CRMCustomerID"].ToString()!,
                FirstName = reader["FirstName"].ToString()!,
                LastName = reader["LastName"].ToString(),
                Email = reader["Email"].ToString()!,
                Phone = reader["Phone"].ToString()!
            };
        }

        // Soft delete
        using var delete = new SqlCommand(@"
            UPDATE Customers
            SET IsDeleted = 1,
                LastUpdated = GETDATE()
            WHERE CRMCustomerID = @id", conn);

        delete.Parameters.AddWithValue("@id", customerId);
        var rows = delete.ExecuteNonQuery();

        // Audit logging
        await _auditService.LogDeleteAsync(
            "Customer",
            customerId,
            deletedCustomer,
            currentUser,
            HttpContext.TraceIdentifier);

        if (rows == 0)
        {
            _logger.LogWarning("Customer delete failed - not found {CRMCustomerID}", customerId);
            throw new NotFoundException("Customer not found");
        }

        _logger.LogInformation("Customer deleted {CRMCustomerID} by {Username}", customerId, currentUser);
        return Ok("Customer deleted successfully");
    }

    #endregion

    #region Debug Endpoints

    /// <summary>
    /// Debug endpoint to list all customer IDs.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("debug/customers/list")]
    public IActionResult DebugCustomers()
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = new SqlCommand("SELECT CRMCustomerID FROM Customers", conn);
        var result = new List<string>();

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(reader["CRMCustomerID"].ToString()!);
        }

        return Ok(result);
    }

    /// <summary>
    /// Debug endpoint to list all customer access entries.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("debug/access")]
    public IActionResult DebugAccess()
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        var list = new List<object>();

        using var cmd = new SqlCommand("SELECT Username, CRMCustomerID FROM CustomerAccess", conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new
            {
                Username = reader["Username"].ToString(),
                CRMCustomerID = reader["CRMCustomerID"].ToString()
            });
        }

        return Ok(list);
    }

    /// <summary>
    /// Debug endpoint to display current user claims.
    /// </summary>
    [Authorize]
    [HttpGet("debug/whoami")]
    public IActionResult WhoAmI()
    {
        return Ok(new
        {
            Name = User.Identity?.Name,
            Authenticated = User.Identity?.IsAuthenticated,
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a JWT token for testing purposes.
    /// </summary>
    private static string CreateJwtToken(string username, string? role, string crmId)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim("username", username),
            new Claim("CRMCustomerID", crmId)
        };

        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-test-secret-key"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Writes a synchronization log entry.
    /// </summary>
    private void WriteSyncLog(string crmCustomerID, string operation, string status, string message, string username)
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
                ExecutionTimeMs = 0
            };

            _context.SyncLogs.Add(log);
            Console.WriteLine("==========================");
            Console.WriteLine($"Connection = {_connectionString}");
            Console.WriteLine("==========================");
            var rows = _context.SaveChanges();
            Console.WriteLine($"SyncLog saved rows={rows}");
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

    #endregion
}