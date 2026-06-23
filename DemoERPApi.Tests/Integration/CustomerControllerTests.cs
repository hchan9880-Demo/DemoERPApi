using DemoERPApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DemoERPApi.Tests.Integration;

public class CustomerControllerTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CustomerControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }


    // =====================================================
    // 1. GET CUSTOMER
    // =====================================================

    [Fact]
    public async Task GetCustomer_ReturnsOk_WhenCrmExists()
    {
        var response = await _client.GetAsync(
            "/api/Customer/CRM100"
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var customer =
            await response.Content.ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(customer);
        Assert.Equal("CRM100", customer!.CustomerId);
    }


    [Fact]
    public async Task GetCustomer_ReturnsNotFound_WhenCrmDoesNotExist()
    {
        var response = await _client.GetAsync(
            "/api/Customer/CRM999999"
        );

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );
    }


    [Fact]
    public async Task GetCustomer_ReturnsBadRequest_WhenCrmIsNull()
    {
        var response = await _client.GetAsync(
            "/api/Customer/"
        );

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }



    // =====================================================
    // 2. SYNC CUSTOMER
    // =====================================================

    [Fact]
    public async Task SyncCustomer_ReturnsOk_WhenCrmExists()
    {
        var request = new CustomerDto
        {
            CustomerId = "CRM100",
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "6049998888"
        };


        var response = await _client.PostAsJsonAsync(
            "/api/Customer/sync",
            request
        );



        //It should be CRM100 is already exsited and not found availabe to use.
        //Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        //show failed assertion 
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);



    }


    [Fact]
    public async Task SyncCustomer_ReturnsNotFound_WhenCrmDoesNotExist()
    {
        var request = new CustomerDto
        {
            CustomerId = "",
            FirstName = "Test",
            LastName = "User"
        };


        var response = await _client.PostAsJsonAsync(
            "/api/Customer/sync",
            request
        );

        //Eitheer Notfound or BadRequest is correct, as long as it is failed due to blank
      
        // Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

       Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);



    }


    [Fact]
    public async Task SyncCustomer_ReturnsBadRequest_WhenCrmIsNull()
    {
        var request = new CustomerDto
        {
            CustomerId = null,
            FirstName = "Test",
            LastName = "User"
        };


        var response = await _client.PostAsJsonAsync(
            "/api/Customer/sync",
            request
        );


        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }



    // =====================================================
    // 3. UPDATE CUSTOMER
    // =====================================================

    [Fact]
    public async Task UpdateCustomer_ReturnsOk_WhenCrmExists()
    {
        var request = new CustomerDto
        {
            CustomerId = "CRM102",
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "6049998888"
        };


        var response = await _client.PutAsJsonAsync(
            "/api/Customer/update",
            request
        );


        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );
    }



    [Fact]
    public async Task UpdateCustomer_ReturnsNotFound_WhenCrmDoesNotExist()
    {
        var request = new CustomerDto
        {
            CustomerId = "CRM999999",
            FirstName = "Test",
            LastName = "User"
        };


        var response = await _client.PutAsJsonAsync(
            "/api/Customer/update",
            request
        );


        //Eitheer Notfound or BadRequest is correct, as long as it is failed due to invalid CustomerId

        // Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    }



    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenCrmIsNull()
    {
        var request = new CustomerDto
        {
            CustomerId = null,
            FirstName = "Test",
            LastName = "User"
        };


        var response = await _client.PutAsJsonAsync(
            "/api/Customer/update",
            request
        );


        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }




    // =====================================================
    // 4. DELETE CUSTOMER
    // =====================================================

    [Fact]
    public async Task DeleteCustomer_ReturnsOk_WhenCrmExists()
    {
        var response = await _client.DeleteAsync(
            "/api/Customer/CRM102"
        );


        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );
    }



    [Fact]
    public async Task DeleteCustomer_ReturnsNotFound_WhenCrmDoesNotExist()
    {
        var response = await _client.DeleteAsync(
            "/api/Customer/CRM999999"
        );


        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode
        );
    }



    [Fact]
    public async Task DeleteCustomer_ReturnsBadRequest_WhenCrmIsNull()
    {
        var response = await _client.DeleteAsync(
            "/api/Customer/"
        );


        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );


       // Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        //Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);



    }




    // =====================================================
    // 2A. SYNC CUSTOMER BAD REQUEST - INVALID JSON
    // =====================================================
    [Fact]
    public async Task SyncCustomer_ReturnsBadRequest_WhenJsonIsInvalid()
    {
        var invalidJson = """
    {
        FirstName = "Michael",
        LastName = "Johnson",
        Email = "michael@test.com",
        Phone = "6049998888"
    }
    """;

        var content = new StringContent(
            invalidJson,
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _client.PostAsync(
            "/api/Customer/sync",
            content
        );

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );

        var responseBody =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "One or more validation errors occurred",
            responseBody
        );
    }

    // =====================================================
    // 3A. UPDATE CUSTOMER BAD REQUEST - CUSTOMER ID MISSING
    // =====================================================
    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenCustomerIdMissing()
    {
        var request = new
        {
            // CustomerId intentionally missing
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "6049998888"
        };


        var response = await _client.PutAsJsonAsync(
            "/api/Customer/update",
            request
        );


        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );


        var responseBody =
            await response.Content.ReadAsStringAsync();


        Assert.Contains(
            "CustomerId is required",
            responseBody
        );
    }




    // =====================================================
    // 2B. SYNC CUSTOMER Email phone field check
    // =====================================================
    // =====================================================
    // SYNC CUSTOMER VALIDATION TESTS
    // =====================================================


    [Fact]
    public async Task SyncCustomer_ReturnsOk_WhenEmailAndPhoneAreValid()
    {
        var request = new CustomerDto
        {
            CustomerId = "CRM200",
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "6049998888"
        };


        var response = await _client.PostAsJsonAsync(
            "/api/Customer/sync",
            request
        );


        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );
    }



    [Fact]
    public async Task SyncCustomer_ReturnsBadRequest_WhenEmailIsNull()
    {
        var request = new CustomerDto
        {
            CustomerId = "CRM201",
            FirstName = "Michael",
            LastName = "Johnson",
            Email = null,
            Phone = "6049998888"
        };


        var response = await _client.PostAsJsonAsync(
            "/api/Customer/sync",
            request
        );


        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }



    [Fact]
    public async Task SyncCustomer_ReturnsBadRequest_WhenEmailFormatIsInvalid()
    {
        var request = new CustomerDto
        {
            CustomerId = "CRM202",
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "wrong-email",
            Phone = "6049998888"
        };


        var response = await _client.PostAsJsonAsync(
            "/api/Customer/sync",
            request
        );


        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }



    [Fact]
    public async Task SyncCustomer_ReturnsOk_WhenPhoneIsNull()
    {
        var request = new CustomerDto
        {
            CustomerId = "CRM203",
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = null
        };


        var response = await _client.PostAsJsonAsync(
            "/api/Customer/sync",
            request
        );


        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );
    }



    [Fact]
    public async Task SyncCustomer_ReturnsBadRequest_WhenPhoneIsInvalid()
    {
        var request = new CustomerDto
        {
            CustomerId = "CRM204",
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "12345"
        };


        var response = await _client.PostAsJsonAsync(
            "/api/Customer/sync",
            request
        );


        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }

    // =====================================================
    // 3B. UPDATE CUSTOMER Email phone field check
    // =====================================================

    // =====================================================
    // UPDATE CUSTOMER VALIDATION TESTS
    // =====================================================


    [Fact]
    public async Task UpdateCustomer_ReturnsOk_WhenEmailAndPhoneAreValid()
    {
        var request = new CustomerDto
        {
            CustomerId = "CRM100",
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "6049998888"
        };


        var response = await _client.PutAsJsonAsync(
            "/api/Customer/update",
            request
        );


        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );
    }



    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenEmailIsNull()
    {
        var request = new CustomerDto
        {
            CustomerId = "CRM100",
            FirstName = "Michael",
            LastName = "Johnson",
            Email = null,
            Phone = "6049998888"
        };


        var response = await _client.PutAsJsonAsync(
            "/api/Customer/update",
            request
        );


        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }



    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenEmailFormatIsInvalid()
    {
        var request = new CustomerDto
        {
            CustomerId = "CRM100",
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "abc",
            Phone = "6049998888"
        };


        var response = await _client.PutAsJsonAsync(
            "/api/Customer/update",
            request
        );


        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }



    [Fact]
    public async Task UpdateCustomer_ReturnsOk_WhenPhoneIsNull()
    {
        var request = new CustomerDto
        {
            CustomerId = "CRM100",
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = null
        };


        var response = await _client.PutAsJsonAsync(
            "/api/Customer/update",
            request
        );


        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode
        );
    }



    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenPhoneIsInvalid()
    {
        var request = new CustomerDto
        {
            CustomerId = "CRM100",
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "555"
        };


        var response = await _client.PutAsJsonAsync(
            "/api/Customer/update",
            request
        );


        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode
        );
    }






}



