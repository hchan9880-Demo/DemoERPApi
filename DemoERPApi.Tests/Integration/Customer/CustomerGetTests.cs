using DemoERPApi.Models;
using DemoERPApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DemoERPApi.Tests.Integration.Customer;

/*
Test Cases Covered:
GET-001	Admin retrieves existing customer	        GET valid customer id
GET-002	Admin requests invalid customer id format	GET null/blank/non-numeric-or-invalid route id per API contract
GET-003	Admin requests non-existent customer	    GET unknown id
GET-004	QA retrieves assigned customer	            GET assigned customer id
GET-005	QA requests invalid customer id	            GET invalid id
GET-006	QA requests non-existent assigned id	    GET unknown id
GET-007	Customer requests invalid customer id	    GET invalid id
GET-008	Request deleted customer record	            GET deleted customer id
*/

public class CustomerGetTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CustomerGetTests(WebApplicationFactory<Program> factory)
    {
        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient();
    }

    // =====================================================
    // GET-001: Admin retrieves existing customer
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Customer exists?
    //      ↓ Yes
    // Retrieve customer
    //      ↓
    // Return 200 OK
    // =====================================================
    [Fact]
    public async Task GET_001_AdminRetrievesExistingCustomer_ReturnsOk()
    {
        TestAuthHelper.SetAdminToken(_client);
        var testId = "GET_TEST_001";
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var response = await _client.GetAsync($"/api/Customer/{testId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<CustomersDto>();
        Assert.NotNull(customer);
        Assert.Equal(testId, customer!.CRMCustomerID);
    }
    // =====================================================
    // GET-002: Admin requests invalid customer id format
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // CustomerId supplied?
    //      ↓ No
    // Route not matched
    //      ↓
    // Return 404 NotFound
    // =====================================================
    [Fact]
    public async Task GET_002_AdminRequestsInvalidCustomerIdFormat_ReturnsNotFound()
    {
        TestAuthHelper.SetAdminToken(_client);

        var response = await _client.GetAsync("/api/Customer/");

        // Returns NotFound due to standard routing matching rules on blank route parameters
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // =====================================================
    // GET-003: Admin requests non-existent customer
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Customer exists?
    //      ↓ No
    // Return 404 NotFound
    // =====================================================
    [Fact]
    public async Task GET_003_AdminRequestsNonExistentCustomer_ReturnsNotFound()
    {
        TestAuthHelper.SetAdminToken(_client);

        var response = await _client.GetAsync($"/api/Customer/{TestData.NonExistingCustomerID}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // =====================================================
    // GET-004: QA retrieves assigned customer
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = QA
    //      ↓
    // Customer exists?
    //      ↓ Yes
    // Customer assigned to QA?
    //      ↓ Yes
    // Retrieve customer
    //      ↓
    // Return 200 OK
    // =====================================================
    [Fact]
    public async Task GET_004_QARetrievesAssignedCustomer_ReturnsOk()
    {
        TestAuthHelper.SetQAToken(_client);
        var testId = "GET_TEST_004";
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var response = await _client.GetAsync($"/api/Customer/{testId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<CustomersDto>();
        Assert.NotNull(customer);
        Assert.Equal(testId, customer!.CRMCustomerID);
    }

    // =====================================================
    // GET-005: QA requests invalid customer id
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = QA
    //      ↓
    // CustomerId supplied?
    //      ↓ No
    // Route not matched
    //      ↓
    // Return 404 NotFound
    // =====================================================
    [Fact]
    public async Task GET_005_QARequestsInvalidCustomerId_ReturnsNotFound()
    {
        TestAuthHelper.SetQAToken(_client);

        var response = await _client.GetAsync("/api/Customer/");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    // =====================================================
    // GET-006: QA requests non-existent assigned id
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = QA
    //      ↓
    // Supported role
    //      ↓
    // Customer exists?
    //      ↓ No
    // Return 404 NotFound
    // =====================================================
    [Fact]
    public async Task GET_006_QARequestsNonExistentAssignedId_ReturnsNotFound()
    {
        TestAuthHelper.SetQAToken(_client);

        var response = await _client.GetAsync($"/api/Customer/{TestData.NonExistingCustomerID}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // =====================================================
    // GET-007: Customer requests invalid customer id
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Customer
    //      ↓
    // CustomerId supplied?
    //      ↓ No
    // Route not matched
    //      ↓
    // Return 404 NotFound
    // =====================================================
    [Fact]
    public async Task GET_007_CustomerRequestsInvalidCustomerId_ReturnsNotFound()
    {
        TestAuthHelper.SetOwnerToken(_client);

        var response = await _client.GetAsync("/api/Customer/");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // =====================================================
    // GET-008: Request deleted customer record
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Customer exists?
    //      ↓ Yes
    // Soft Delete Customer
    //      ↓
    // Execute GET request
    //      ↓
    // Deleted customer filtered out
    //      ↓
    // Return 404 NotFound
    // =====================================================
    [Fact]
    public async Task GET_008_RequestDeletedCustomerRecord_ReturnsNotFound()
    {
        TestAuthHelper.SetAdminToken(_client);
        var testId = "GET_TEST_008";
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        // Perform Soft Delete via the API to flag the record
        var deleteResponse = await _client.DeleteAsync($"/api/Customer/{testId}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // Assert that subsequent GET queries find it unavailable
        var response = await _client.GetAsync($"/api/Customer/{testId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }



    // =====================================================
    // BASELINE SECURITY PATHS
    // =====================================================
    // SECURITY-GET-009: Customer retrieves another user's record
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Customer
    //      ↓
    // Requested customer is owner?
    //      ↓ No
    // Authorization fails
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact]
    public async Task GET_009_GetCustomer_ReturnsForbidden_WhenOwnerRequestsAnotherUsersCustomer()
    {
        TestAuthHelper.SetOwnerToken(_client);
        var testId = TestData.OtherCustomerID;

        var response = await _client.GetAsync($"/api/Customer/{testId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // SECURITY-GET-010: Customer retrieves own record
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Customer
    //      ↓
    // Requested customer is owner?
    //      ↓ Yes
    // Retrieve customer
    //      ↓
    // Return 200 OK
    // =====================================================
    [Fact]
    public async Task GET_010_GetCustomer_ReturnsOk_WhenOwnerRequestsOwnCustomer()
    {
        TestAuthHelper.SetOwnerToken(_client);
        var testId = TestData.OwnerCustomerID;

        var response = await _client.GetAsync($"/api/Customer/{testId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    // =====================================================
    // SECURITY-GET-011: Invalid JWT token
    //
    // Workflow:
    // JWT supplied
    //      ↓
    // JWT validation fails
    //      ↓
    // Authentication fails
    //      ↓
    // Return 401 Unauthorized
    // =====================================================
    [Fact]
    public async Task GET_011_GetCustomer_ReturnsUnauthorized_WhenJwtInvalid()
    {
        TestAuthHelper.SetInvalidToken(_client);

        var response = await _client.GetAsync($"/api/Customer/{TestData.ExistingCustomerID}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }


    // =====================================================
    // SECURITY-GET-012: Missing JWT token
    //
    // Workflow:
    // JWT supplied?
    //      ↓ No
    // Authentication fails
    //      ↓
    // Return 401 Unauthorized
    // =====================================================
    [Fact]
    public async Task GET_012_GetCustomer_ReturnsUnauthorized_WhenJwtMissing()
    {
        TestAuthHelper.ClearToken(_client);

        var response = await _client.GetAsync($"/api/Customer/{TestData.ExistingCustomerID}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }





}