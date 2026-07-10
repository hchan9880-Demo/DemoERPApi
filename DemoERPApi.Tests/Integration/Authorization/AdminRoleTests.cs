using DemoERPApi.Models;
using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using DemoERPApi.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace DemoERPApi.Tests.Integration.Authorization;

/*
Test Cases :
AUTH-011a    /api/Customer/{id}    GET       Admin retrieves customer owned by QA/customer       -> 200 OK
AUTH-011b    /api/Customer/{id}    PUT       Admin updates another user's customer profile       -> 200 OK
AUTH-011c    /api/Customer/{id}    DELETE    Admin deletes another user's customer account       -> 200 OK
AUTH-011d    /api/Customer/sync    POST      Admin synchronizes/creates a new customer record    -> 200 OK
AUTH-011e    /api/Customer/{id}    GET       Admin retrieves own customer record                 -> 200 OK
AUTH-011f    /api/Customer/{id}    PUT       Admin updates Admin user's customer profile         -> 200 OK
AUTH-011g    /api/Customer/{id}    DELETE    Admin deletes own customer account                  -> 200 OK
*/

public class AdminRoleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AdminRoleTests(WebApplicationFactory<Program> factory)
    {
        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient();
    }

    private static CustomerDto GetValidPayload(string customerId) => new()
    {
        CRMCustomerID = customerId,
        FirstName = "Admin",
        LastName = "User Data",
        Email = "admin.test@test.com",
        Phone = "6045551212"
    };

    // ===================================================================================
    // AUTH-011a: Admin retrieves customer owned by QA/customer
    // ===================================================================================
    [Fact]
    public async Task AUTH_011a_AdminRetrievesCustomerOwnedByAnotherRole_ReturnsOk()
    {
        var testId = "CRM_ADMIN_GET_011A";

        TestAuthHelper.SetAdminToken(_client);
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var response = await _client.GetAsync($"/api/Customer/{testId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        Assert.NotNull(customer);
        Assert.Equal(testId, customer.CRMCustomerID);
    }

    // ===================================================================================
    // AUTH-011b: Admin updates another user's customer profile
    // ===================================================================================
    [Fact]
    public async Task AUTH_011b_AdminUpdatesAnotherCustomerProfile_ReturnsOk()
    {
        var testId = "CRM_ADMIN_PUT_011B";

        TestAuthHelper.SetAdminToken(_client);
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var requestPayload = GetValidPayload(testId);
        requestPayload.FirstName = "AdminModifiedExternal";

        var response = await _client.PutAsJsonAsync($"/api/Customer/{testId}", requestPayload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ===================================================================================
    // AUTH-011c: Admin deletes another user's customer account
    // ===================================================================================
    [Fact]
    public async Task AUTH_011c_AdminDeletesAnotherCustomerAccount_ReturnsOk()
    {
        var testId = "CRM_ADMIN_DEL_011C";

        TestAuthHelper.SetAdminToken(_client);
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var response = await _client.DeleteAsync($"/api/Customer/{testId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ===================================================================================
    // AUTH-011d: Admin synchronizes/creates a new customer record
    // ===================================================================================
    [Fact]
    public async Task AUTH_011d_AdminAttemptsToSyncCustomer_ReturnsOk()
    {
        var testId = "CRM_ADMIN_SYNC_011D";
        var dbHelper = new TestDatabaseHelper();
        await dbHelper.DeleteCustomer(testId);

        TestAuthHelper.SetAdminToken(_client);
        var requestPayload = GetValidPayload(testId);

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", requestPayload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ===================================================================================
    // AUTH-011e: Admin retrieves own customer record
    // ===================================================================================
    [Fact]
    public async Task AUTH_011e_AdminRetrievesOwnCustomerRecord_ReturnsOk()
    {
        var testId = "CRM_ADMIN_GET_011E";

        TestAuthHelper.SetAdminToken(_client);
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var response = await _client.GetAsync($"/api/Customer/{testId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        Assert.NotNull(customer);
        Assert.Equal(testId, customer.CRMCustomerID);
    }

    // ===================================================================================
    // AUTH-011f: Admin updates Admin user's customer profile
    // ===================================================================================
    [Fact]
    public async Task AUTH_011f_AdminUpdatesOwnCustomerProfile_ReturnsOk()
    {
        var testId = "CRM_ADMIN_PUT_011F";

        TestAuthHelper.SetAdminToken(_client);
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var requestPayload = GetValidPayload(testId);
        requestPayload.FirstName = "AdminSelfUpdated";

        var response = await _client.PutAsJsonAsync($"/api/Customer/{testId}", requestPayload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ===================================================================================
    // AUTH-011g: Admin deletes own customer account
    // ===================================================================================
    [Fact]
    public async Task AUTH_011g_AdminDeletesOwnCustomerAccount_ReturnsOk()
    {
        var testId = "CRM_ADMIN_DEL_011G";

        TestAuthHelper.SetAdminToken(_client);
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var response = await _client.DeleteAsync($"/api/Customer/{testId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}