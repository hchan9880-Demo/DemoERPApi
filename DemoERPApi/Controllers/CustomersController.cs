/*
 
using DemoERPApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DemoERPApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    [HttpGet]
    public ActionResult<CustomerDto> GetCustomer()
    {
        return new CustomerDto
        {
            CustomerId = "C001",
            FirstName = "John",
            LastName = "Smith",
            Email = "john.smith@example.com",
            Phone = "555-1234"
        };
    }
}

*/
/*

using DemoERPApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DemoERPApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public CustomerController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("sync")]
    public IActionResult SyncCustomer(CustomerDto customer)
    {
        string connectionString =
            _configuration.GetConnectionString("Default");

        using SqlConnection connection =
            new SqlConnection(connectionString);

        using SqlCommand command =
            new SqlCommand("usp_InsertCustomer", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue(
            "@CRMCustomerID",
            customer.CustomerId);

        command.Parameters.AddWithValue(
            "@FirstName",
            customer.FirstName);

        command.Parameters.AddWithValue(
            "@LastName",
            customer.LastName);

        command.Parameters.AddWithValue(
            "@Email",
            customer.Email);

        command.Parameters.AddWithValue(
            "@Phone",
            customer.Phone);

        connection.Open();
        command.ExecuteNonQuery();

        return Ok("Synchronization Complete");
    }
}
*/


using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using DemoERPApi.Models;

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
    public IActionResult SyncCustomer(CustomerDto customer)
    {
        using SqlConnection conn = new SqlConnection(_connectionString);

        conn.Open();

        return Ok(new
        {
            Message = "Customer synced successfully",
            Customer = customer
        });
    }
}