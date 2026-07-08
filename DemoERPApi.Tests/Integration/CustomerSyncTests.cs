using DemoERPApi.Models;
using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace DemoERPApi.Tests.Integration;

public class CustomerSyncTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly TestDatabaseHelper _db;

    private const string TEST_ID_1 = "CRM300";
    private const string TEST_ID_2 = "CRM301";





    public CustomerSyncTests(WebApplicationFactory<Program> factory)
    {
        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient();

        _db = new TestDatabaseHelper();
    }

    // =====================================================
    // 1. ADMIN - VALID CREATE
    // =====================================================
    [Fact]
    public async Task SyncCustomer_ReturnsOk_WhenAdminCreatesValidCustomer()
    {
        await _db.DeleteCustomer(TEST_ID_1);
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = TEST_ID_1,
            FirstName = "John",
            LastName = "Smith",
            Email = "john300@test.com",
            Phone = "6041234567"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // =====================================================
    // 2. AUTHORIZED USER (CUSTOMER ROLE)
    // =====================================================
    [Fact]
    public async Task SyncCustomer_ReturnsOk_WhenAuthorizedUserCreatesValidCustomer()
    {
        await _db.DeleteCustomer(TEST_ID_2);
        TestAuthHelper.SetOwnerToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = TEST_ID_2,
            FirstName = "Alice",
            LastName = "Brown",
            Email = "alice301@test.com",
            Phone = "6041234568"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // =====================================================
    // 3. UNAUTHORIZED ROLE
    // =====================================================
    [Fact]
    public async Task SyncCustomer_ReturnsForbidden_WhenUnauthorizedRoleCreatesCustomer()
    {
        TestAuthHelper.SetUnauthorizedUserToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = "CRM400",
            FirstName = "Guest",
            LastName = "User",
            Email = "guest@test.com",
            Phone = "6041234569"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // =====================================================
    // 4. NO JWT
    // =====================================================
    [Fact]
    public async Task SyncCustomer_ReturnsUnauthorized_WhenJwtMissing()
    {
        TestAuthHelper.ClearToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = "CRM401",
            FirstName = "No",
            LastName = "Token",
            Email = "notoken@test.com",
            Phone = "6041111111"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // =====================================================
    // 5. INVALID JWT
    // =====================================================
    [Fact]
    public async Task SyncCustomer_ReturnsUnauthorized_WhenJwtInvalid()
    {
        TestAuthHelper.SetInvalidToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = "CRM402",
            FirstName = "Bad",
            LastName = "Token",
            Email = "bad@test.com",
            Phone = "6042222222"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // =====================================================
    // 6. DUPLICATE CUSTOMER
    // =====================================================
    [Fact]
    public async Task SyncCustomer_ReturnsConflict_WhenCustomerAlreadyExists()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = TestData.ExistingCustomerID,
            FirstName = "Michael",
            LastName = "Johnson",
            Email = "michael@test.com",
            Phone = "6049998888"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // =====================================================
    // 7. MISSING CUSTOMER ID
    // =====================================================
    [Fact]
    public async Task SyncCustomer_ReturnsBadRequest_WhenCustomerIDIsMissing()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = null,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            Phone = "6049991111"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =====================================================
    // 8. MISSING FIRST NAME
    // =====================================================
    [Fact]
    public async Task SyncCustomer_ReturnsBadRequest_WhenFirstNameIsMissing()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = "CRM405",
            FirstName = null,
            LastName = "User",
            Email = "test405@test.com",
            Phone = "6049992222"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =====================================================
    // 9. MISSING EMAIL
    // =====================================================
    [Fact]
    public async Task SyncCustomer_ReturnsBadRequest_WhenEmailIsMissing()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = "CRM406",
            FirstName = "Test",
            LastName = "User",
            Email = null,
            Phone = "6049993333"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =====================================================
    // 10. INVALID EMAIL
    // =====================================================
    [Fact]
    public async Task SyncCustomer_ReturnsBadRequest_WhenEmailIsInvalid()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = "CRM403",
            FirstName = "Test",
            LastName = "User",
            Email = "wrong-email",
            Phone = "6049994444"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =====================================================
    // 11. INVALID PHONE
    // =====================================================
    [Fact]
    public async Task SyncCustomer_ReturnsBadRequest_WhenPhoneIsInvalid()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = "CRM404",
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            Phone = "12345"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =====================================================
    // 12. NULL PHONE
    // =====================================================
    [Fact]
    public async Task SyncCustomer_ReturnsBadRequest_WhenPhoneIsNull()
    {
        TestAuthHelper.SetAdminToken(_client);

        var request = new CustomerDto
        {
            CRMCustomerID = "CRM409",
            FirstName = "Test",
            LastName = "User",
            Email = "test409@test.com",
            Phone = null
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =====================================================
    // 13. INVALID JSON
    // =====================================================
    [Fact]
    public async Task SyncCustomer_ReturnsBadRequest_WhenJsonIsInvalid()
    {
        TestAuthHelper.SetAdminToken(_client);

        var invalidJson = """
        {
            FirstName: "Michael",
            LastName: "Johnson",
            Email: "michael@test.com",
            Phone: "6049998888"
        }
        """;

        var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Customer/sync", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}