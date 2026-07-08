/*
This should contain the 12 PUT tests:

Admin valid update → 200
Owner own record → 200
Owner other record → 403
invalid customer → 404
missing CustomerID → 400
missing FirstName → 400
missing Email → 400
invalid Email → 400
invalid Phone → 400
null Phone → 200 or 400 depending on your actual validation
missing JWT → 401
invalid JWT → 401
 
 */


using Azure;
using DemoERPApi.Models;
using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using DemoERPApi.Tests.TestHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DemoERPApi.Tests.Integration;

public class CustomerUpdateTests
: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;


    public CustomerUpdateTests(WebApplicationFactory<Program> factory)
    {
        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient();

        var helper = new TestDatabaseHelper();
        helper.ResetDatabase().GetAwaiter().GetResult();
        helper.SeedCustomers().GetAwaiter().GetResult();
    }

    // =====================================================
    // PUT /api/Customer/update
    // 12 tests
    // =====================================================

    [Fact]
    public async Task UpdateCustomer_ReturnsOk_WhenAdminUpdatesValidCustomer()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = TestData.ExistingCustomerID2,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "6049998888"
        };


        var response = await _client.PutAsJsonAsync(
    $"/api/Customer/{TestData.ExistingCustomerID2}",
    request
);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsOk_WhenOwnerUpdatesOwnCustomer()
    {
        TestAuthHelper.SetOwnerToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = TestData.OwnerCustomerID,
            FirstName = "Owner",
            LastName = "Updated",
            Email = "owner@test.com",
            Phone = "6048887777"
        };


        var response = await _client.PutAsJsonAsync(
    $"/api/Customer/{TestData.OwnerCustomerID}",
    request
);


        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsForbidden_WhenOwnerUpdatesAnotherUsersCustomer()
    {
        TestAuthHelper.SetOwnerToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = TestData.OtherCustomerID,
            FirstName = "Other",
            LastName = "User",
            Email = "other@test.com",
            Phone = "6047776666"
        };


        var response = await _client.PutAsJsonAsync(
    $"/api/Customer/{TestData.OtherCustomerID}",
    request
);



        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = TestData.NonExistingCustomerID,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            Phone = "6041112222"
        };

        var response = await _client.PutAsJsonAsync(
    $"/api/Customer/{TestData.NonExistingCustomerID}",
    request
);



        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsNotFound_WhenCustomerIDIsMissing()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = null,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "6049998888",
            IsDeleted = false
        };

        var response = await _client.PutAsJsonAsync(
    $"/api/Customer/",
          request
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenFirstNameIsMissing()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = TestData.ExistingCustomerID,
            FirstName = null,
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "6049998888"
        };

        
                    var response = await _client.PutAsJsonAsync(
    $"/api/Customer/{TestData.ExistingCustomerID}",
    request
);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenEmailIsMissing()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = TestData.ExistingCustomerID,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = null,
            Phone = "6049998888"
        };

        var response = await _client.PutAsJsonAsync(
$"/api/Customer/{TestData.ExistingCustomerID}",
request
);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenEmailFormatIsInvalid()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = TestData.ExistingCustomerID,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "abc",
            Phone = "6049998888",
            IsDeleted = false
        };

        var response = await _client.PutAsJsonAsync(
$"/api/Customer/{TestData.ExistingCustomerID}",
request
);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    [Authorize(Roles = "Admin")]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenPhoneIsInvalid()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = TestData.ExistingCustomerID,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "555",
            IsDeleted = false
        };

        var response = await _client.PutAsJsonAsync(
            $"/api/Customer/{TestData.ExistingCustomerID}",
            request
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsBadRequest_WhenPhoneIsNull()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = TestData.ExistingCustomerID,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = null,
            IsDeleted = false
        };

        var response = await _client.PutAsJsonAsync(
$"/api/Customer/{TestData.ExistingCustomerID}",
request
);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsUnauthorized_WhenJwtMissing()
    {
        TestAuthHelper.ClearToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = TestData.ExistingCustomerID,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "6049998888",
            IsDeleted = false
        };

        var response = await _client.PutAsJsonAsync(
$"/api/Customer/{TestData.ExistingCustomerID}",
request
);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsUnauthorized_WhenJwtInvalid()
    {
        TestAuthHelper.SetInvalidToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = TestData.ExistingCustomerID,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "6049998888",
            IsDeleted = false
        };

        var response = await _client.PutAsJsonAsync(
$"/api/Customer/{TestData.ExistingCustomerID}",
request
);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }


}
