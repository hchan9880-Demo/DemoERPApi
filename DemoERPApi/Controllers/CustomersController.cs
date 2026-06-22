using DemoERPApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DemoERPApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly string _connectionString;

    public CustomerController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DemoERPConnection");
    }

    // =====================================================
    // ORIGINAL FUNCTION (UNCHANGED - DO NOT TOUCH)
    // =====================================================
    [HttpPost("sync")]
    public IActionResult SyncCustomer([FromBody] CustomerDto customer)
    {
        if (customer == null)
        {
            return BadRequest("Customer payload is null");
        }

        try
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            using SqlCommand cmd = new SqlCommand("usp_InsertCustomer", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@CRMCustomerID", SqlDbType.VarChar).Value = customer.CustomerId ?? "";
            cmd.Parameters.Add("@FirstName", SqlDbType.VarChar).Value = customer.FirstName ?? "";
            cmd.Parameters.Add("@LastName", SqlDbType.VarChar).Value = customer.LastName ?? "";
            cmd.Parameters.Add("@Email", SqlDbType.VarChar).Value = customer.Email ?? "";
            cmd.Parameters.Add("@Phone", SqlDbType.VarChar).Value = customer.Phone ?? "";

            conn.Open();

            int rowsAffected = cmd.ExecuteNonQuery();

            return Ok(new
            {
                Message = "Customer synced successfully",
                RowsAffected = rowsAffected
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = ex.Message,
                Detail = ex.InnerException?.Message
            });
        }
    }

    // =====================================================
    // NEW: GET CUSTOMER BY ID
    // =====================================================
    [HttpGet("{crmId}")]
    public IActionResult GetCustomer(string crmId)
    {
        try
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"
                SELECT CRMCustomerID, FirstName, LastName, Email, Phone
                FROM Customer
                WHERE CRMCustomerID = @CRMCustomerID";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CRMCustomerID", crmId);

            conn.Open();

            using SqlDataReader reader = cmd.ExecuteReader();

            if (!reader.Read())
                return NotFound(new { Message = "Customer not found" });

            return Ok(new CustomerDto
            {
                CustomerId = reader["CRMCustomerID"].ToString(),
                FirstName = reader["FirstName"].ToString(),
                LastName = reader["LastName"].ToString(),
                Email = reader["Email"].ToString(),
                Phone = reader["Phone"].ToString()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    // =====================================================
    // NEW: UPDATE CUSTOMER
    // =====================================================
    [HttpPut("update")]
    public IActionResult UpdateCustomer([FromBody] CustomerDto customer)
    {
        if (customer == null || string.IsNullOrEmpty(customer.CustomerId))
            return BadRequest("Invalid customer data");

        try
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Customer
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    Phone = @Phone
                WHERE CRMCustomerID = @CRMCustomerID";

            using SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@CRMCustomerID", customer.CustomerId);
            cmd.Parameters.AddWithValue("@FirstName", customer.FirstName);
            cmd.Parameters.AddWithValue("@LastName", customer.LastName);
            cmd.Parameters.AddWithValue("@Email", customer.Email);
            cmd.Parameters.AddWithValue("@Phone", customer.Phone);

            conn.Open();

            int rows = cmd.ExecuteNonQuery();

            if (rows == 0)
                return NotFound(new { Message = "Customer not found" });

            return Ok(new { Message = "Customer updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    // =====================================================
    // NEW: SOFT DELETE CUSTOMER
    // =====================================================
    [HttpDelete("{crmId}")]
    public IActionResult DeleteCustomer(string crmId)
    {
        try
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Customer
                SET IsDeleted = 1
                WHERE CRMCustomerID = @CRMCustomerID";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CRMCustomerID", crmId);

            conn.Open();

            int rows = cmd.ExecuteNonQuery();

            if (rows == 0)
                return NotFound(new { Message = "Customer not found" });

            return Ok(new { Message = "Customer soft deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }
}