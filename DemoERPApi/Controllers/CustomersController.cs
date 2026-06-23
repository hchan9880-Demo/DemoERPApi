/*
 
Main fixes in MileStone3:

GET and DELETE routes changed to {crmId?} so null reaches controller.
SyncCustomer now checks if customer exists before insert.
Nullable parameters added.

Validation checker:
Required CustomerId
Required FirstName
Required Email
Valid email format check
Phone must be exactly 10 digits
Applies to both:
POST /api/Customer/sync
PUT /api/Customer/update
Keeps the previous fixes:
{crmId?} routes for GET/DELETE null validation
duplicate CRM check before insert

*/

using DemoERPApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

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
            ?? throw new InvalidOperationException(
                "DemoERPConnection is missing in appsettings.json");
    }



    // =====================================================
    // VALIDATION HELPERS
    // =====================================================

    private bool IsValidEmail(string email)
    {
        return Regex.IsMatch(
            email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
        );
    }


    private bool IsValidPhone(string phone)
    {
        return Regex.IsMatch(
            phone,
            @"^\d{10}$"
        );
    }




    // =====================================================
    // CREATE / SYNC CUSTOMER
    // =====================================================

    [HttpPost("sync")]
    public IActionResult SyncCustomer(
        [FromBody] CustomerDto customer)
    {

        if (customer == null)
        {
            return BadRequest(new
            {
                Message = "Customer payload is required"
            });
        }


        if (string.IsNullOrWhiteSpace(customer.CustomerId))
        {
            return BadRequest(new
            {
                Message = "CustomerId is required"
            });
        }


        if (string.IsNullOrWhiteSpace(customer.FirstName))
        {
            return BadRequest(new
            {
                Message = "FirstName is required"
            });
        }


        if (string.IsNullOrWhiteSpace(customer.Email))
        {
            return BadRequest(new
            {
                Message = "Email is required"
            });
        }


        if (!IsValidEmail(customer.Email))
        {
            return BadRequest(new
            {
                Message = "Invalid email format"
            });
        }


        if (!string.IsNullOrWhiteSpace(customer.Phone)
            && !IsValidPhone(customer.Phone))
        {
            return BadRequest(new
            {
                Message =
                "Phone number must contain exactly 10 digits"
            });
        }



        try
        {
            using SqlConnection conn =
                new SqlConnection(_connectionString);


            conn.Open();



            using SqlCommand check =
                new SqlCommand(
                @"SELECT COUNT(*)
                  FROM Customer
                  WHERE CRMCustomerID=@CRMCustomerID",
                conn);



            check.Parameters.Add("@CRMCustomerID",
                SqlDbType.VarChar)
                .Value = customer.CustomerId;



            int exists =
                (int)check.ExecuteScalar();



            if (exists > 0)
            {
                return Conflict(new
                {
                    Message = "Customer already exists"
                });
            }





            using SqlCommand cmd =
                new SqlCommand(
                "usp_InsertCustomer",
                conn);


            cmd.CommandType =
                CommandType.StoredProcedure;



            cmd.Parameters.Add("@CRMCustomerID",
                SqlDbType.VarChar)
                .Value = customer.CustomerId;


            cmd.Parameters.Add("@FirstName",
                SqlDbType.VarChar)
                .Value = customer.FirstName;


            cmd.Parameters.Add("@LastName",
                SqlDbType.VarChar)
                .Value = customer.LastName ?? "";


            cmd.Parameters.Add("@Email",
                SqlDbType.VarChar)
                .Value = customer.Email;


            cmd.Parameters.Add("@Phone",
                SqlDbType.VarChar)
                .Value = customer.Phone ?? "";



            int rows =
                cmd.ExecuteNonQuery();



            return Ok(new
            {
                Message = "Customer synced successfully",
                RowsAffected = rows
            });

        }
        catch (SqlException ex)
        {
            return StatusCode(500, new
            {
                Message = "Database error",
                Error = ex.Message
            });
        }
    }






    // =====================================================
    // GET CUSTOMER
    // =====================================================

    [HttpGet("{crmId?}")]
    public IActionResult GetCustomer(
        string? crmId)
    {

        if (string.IsNullOrWhiteSpace(crmId))
        {
            return BadRequest(new
            {
                Message =
                "CRM customer id is required"
            });
        }



        try
        {
            using SqlConnection conn =
                new SqlConnection(_connectionString);



            string sql = @"
            SELECT CRMCustomerID,
                   FirstName,
                   LastName,
                   Email,
                   Phone
            FROM Customer
            WHERE CRMCustomerID=@CRMCustomerID";



            using SqlCommand cmd =
                new SqlCommand(sql, conn);



            cmd.Parameters.Add("@CRMCustomerID",
                SqlDbType.VarChar)
                .Value = crmId;



            conn.Open();



            using SqlDataReader reader =
                cmd.ExecuteReader();



            if (!reader.Read())
            {
                return NotFound(new
                {
                    Message = "Customer not found"
                });
            }



            return Ok(new CustomerDto
            {
                CustomerId =
                    reader["CRMCustomerID"].ToString(),

                FirstName =
                    reader["FirstName"].ToString(),

                LastName =
                    reader["LastName"].ToString(),

                Email =
                    reader["Email"].ToString(),

                Phone =
                    reader["Phone"].ToString()
            });

        }
        catch (SqlException ex)
        {
            return StatusCode(500, new
            {
                Message = "Database error",
                Error = ex.Message
            });
        }
    }






    // =====================================================
    // UPDATE CUSTOMER
    // =====================================================

    [HttpPut("update")]
    public IActionResult UpdateCustomer(
        [FromBody] CustomerDto customer)
    {

        if (customer == null)
        {
            return BadRequest(new
            {
                Message = "Customer payload required"
            });
        }



        if (string.IsNullOrWhiteSpace(customer.CustomerId))
        {
            return BadRequest(new
            {
                Message = "CustomerId is required"
            });
        }



        if (string.IsNullOrWhiteSpace(customer.Email))
        {
            return BadRequest(new
            {
                Message = "Email is required"
            });
        }



        if (!IsValidEmail(customer.Email))
        {
            return BadRequest(new
            {
                Message = "Invalid email format"
            });
        }



        if (!string.IsNullOrWhiteSpace(customer.Phone)
            && !IsValidPhone(customer.Phone))
        {
            return BadRequest(new
            {
                Message =
                "Phone number must contain exactly 10 digits"
            });
        }


        if (string.IsNullOrWhiteSpace(customer.FirstName))
        {
            return BadRequest(new
            {
                Message = "FirstName is required"
            });
        }

        try
        {
            using SqlConnection conn =
                new SqlConnection(_connectionString);



            string sql = @"
            UPDATE Customer
            SET
            FirstName=@FirstName,
            LastName=@LastName,
            Email=@Email,
            Phone=@Phone
            WHERE CRMCustomerID=@CRMCustomerID";



            using SqlCommand cmd =
                new SqlCommand(sql, conn);



            cmd.Parameters.Add("@CRMCustomerID",
                SqlDbType.VarChar)
                .Value = customer.CustomerId;


            cmd.Parameters.Add("@FirstName",
                SqlDbType.VarChar)
                .Value = customer.FirstName ?? "";


            cmd.Parameters.Add("@LastName",
                SqlDbType.VarChar)
                .Value = customer.LastName ?? "";


            cmd.Parameters.Add("@Email",
                SqlDbType.VarChar)
                .Value = customer.Email;


            cmd.Parameters.Add("@Phone",
                SqlDbType.VarChar)
                .Value = customer.Phone ?? "";



            conn.Open();



            int rows =
                cmd.ExecuteNonQuery();



            if (rows == 0)
            {
                return NotFound(new
                {
                    Message = "Customer not found"
                });
            }



            return Ok(new
            {
                Message = "Customer updated successfully"
            });

        }
        catch (SqlException ex)
        {
            return StatusCode(500, new
            {
                Message = "Database error",
                Error = ex.Message
            });
        }
    }






    // =====================================================
    // DELETE CUSTOMER
    // =====================================================

    [HttpDelete("{crmId?}")]
    public IActionResult DeleteCustomer(
        string? crmId)
    {

        if (string.IsNullOrWhiteSpace(crmId))
        {
            return BadRequest(new
            {
                Message =
                "CRM customer id is required"
            });
        }



        try
        {
            using SqlConnection conn =
                new SqlConnection(_connectionString);



            string sql = @"
            UPDATE Customer
            SET IsDeleted=1
            WHERE CRMCustomerID=@CRMCustomerID";



            using SqlCommand cmd =
                new SqlCommand(sql, conn);



            cmd.Parameters.Add("@CRMCustomerID",
                SqlDbType.VarChar)
                .Value = crmId;



            conn.Open();



            int rows =
                cmd.ExecuteNonQuery();



            if (rows == 0)
            {
                return NotFound(new
                {
                    Message = "Customer not found"
                });
            }



            return Ok(new
            {
                Message =
                "Customer soft deleted successfully"
            });

        }
        catch (SqlException ex)
        {
            return StatusCode(500, new
            {
                Message = "Database error",
                Error = ex.Message
            });
        }
    }
}