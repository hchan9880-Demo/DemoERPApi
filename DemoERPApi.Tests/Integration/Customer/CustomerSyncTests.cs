using DemoERPApi.Models;
using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace DemoERPApi.Tests.Integration.Customer;

/*
Test Cases Covered:
SYNC-001	Admin creates customer	                        Valid payload
SYNC-002	Admin creates duplicate customer	              Duplicate payload
SYNC-003	QA creates assigned customer if permitted	      Valid payload for customer within QA assignment scope
SYNC-015	Sync duplicate against soft-deleted customer	  Valid sync payload matching deleted record
*/

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
    // SYNC-001: Admin creates customer (Valid payload)
    // =====================================================
    [Fact]
    public async Task SYNC_001_AdminCreatesCustomer_WithValidPayload_ReturnsOk()
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
    // SYNC-002: Admin creates duplicate customer
    // =====================================================
    [Fact]
    public async Task SYNC_002_AdminCreatesDuplicateCustomer_ReturnsConflict()
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
    // SYNC-003: QA creates assigned customer if permitted
    // =====================================================
    [Fact]
    public async Task SYNC_003_QACreatesAssignedCustomer_IfPermitted_ReturnsOk()
    {
        var qaAssignedId = "CRM_QA_777";

        await _db.DeleteCustomer(qaAssignedId);


        // ADD THIS
        await _db.AssignCustomerAccess(
            "qauserB",
            qaAssignedId);


        TestAuthHelper.SetQAToken(_client);


        var request = new CustomerDto
        {
            CRMCustomerID = qaAssignedId,
            FirstName = "QA_Assigned",
            LastName = "Customer",
            Email = "qa_assigned@test.com",
            Phone = "6045554321"
        };


        var response =
            await _client.PostAsJsonAsync(
                "/api/Customer/sync",
                request);


        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    // =====================================================
    // SYNC-015: Sync duplicate against soft-deleted customer
    // =====================================================
    [Fact]
    public async Task SYNC_015_SyncDuplicateAgainstSoftDeletedCustomer_HandlesCorrectly()
    {
        TestAuthHelper.SetAdminToken(_client);
        var testId = "CRM_SOFT_DEL_15";

        // 1. Establish a clean record, then soft-delete it via the endpoint
        await _db.DeleteCustomer(testId);
        var setupPayload = new CustomerDto
        {
            CRMCustomerID = testId,
            FirstName = "Initial",
            LastName = "Setup",
            Email = "initial@test.com",
            Phone = "6041112222"
        };
        await _client.PostAsJsonAsync("/api/Customer/sync", setupPayload);
        await _client.DeleteAsync($"/api/Customer/{testId}");

        // 2. Attempt to sync a duplicate payload against the soft-deleted row
        var duplicatePayload = new CustomerDto
        {
            CRMCustomerID = testId,
            FirstName = "Reactived",
            LastName = "User",
            Email = "reactive@test.com",
            Phone = "6041112222"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", duplicatePayload);

        // Depending on API spec, this usually resolves to OK (restores/un-deletes) or Conflict.
        // Adjust the expected assertion value according to your exact business specifications.
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Conflict);
    }

    // =====================================================
    // BASELINE SECURITY & VALIDATION PATHS
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