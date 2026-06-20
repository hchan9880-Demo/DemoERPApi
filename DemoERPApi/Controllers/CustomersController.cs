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

            // safer parameter handling (avoid null crashes)
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
}