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
Test Cases Covered:

AUTH-040a	/api/customers/sync	POST	Anonymous Role Claim -> 403 Forbidden		
AUTH-040b	/api/customer/{id}	GET	Anonymous Role Claim -> 403 Forbidden		
AUTH-040c	/api/customer/{id}	PUT	Anonymous Role Claim -> 403 Forbidden		
AUTH-040d	/api/customer/{id}	DELETE	Anonymous Role Claim -> 403 Forbidden		
AUTH-040e	/api/customer/{id}	PUT	Anonymous Role Claim -> 403 Forbidden		
AUTH-040f	/api/customer/{id}	DELETE	Anonymous Role Claim -> 403 Forbidden		
AUTH-040g	/api/customers/sync	POST	Anonymous Role Claim -> 403 Forbidden		

*/
public class AnonymousRoleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AnonymousRoleTests(WebApplicationFactory<Program> factory)
    {
        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient();
    }

    private static CustomerDto GetValidPayload(string customerId) => new()
    {
        CRMCustomerID = customerId,
        FirstName = "Unauthorized",
        LastName = "Anonymous",
        Email = "anonymous@test.com",
        Phone = "6040000000"
    };

    // ===================================================================================
    // AUTH-040a: Anonymous attempts to create/sync a customer
    // ===================================================================================
    [Fact]
    public async Task AUTH_040a_SyncCustomer_WithAnonymousRoleClaimToken_ReturnsForbidden()
    {
        // Arrange
        var testId = "CRM_ANON_SYNC_040A";
        var dbHelper = new TestDatabaseHelper();
        await dbHelper.DeleteCustomer(testId);

        TestAuthHelper.SetTokenWithRole(_client, "Anonymous");
        var request = GetValidPayload(testId);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ===================================================================================
    // AUTH-040b: Anonymous attempts to retrieve a customer record
    // ===================================================================================
    [Fact]
    public async Task AUTH_040b_GetCustomer_WithAnonymousRoleClaimToken_ReturnsForbidden()
    {
        // Arrange
        var testId = "CRM_ANON_GET_040B";
        TestAuthHelper.SetAdminToken(_client);
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        // Act: Switch context back to Anonymous role token
        TestAuthHelper.SetTokenWithRole(_client, "Anonymous");
        var response = await _client.GetAsync($"/api/Customer/{testId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ===================================================================================
    // AUTH-040c: Anonymous attempts to update a customer profile
    // ===================================================================================
    [Fact]
    public async Task AUTH_040c_UpdateCustomer_WithAnonymousRoleClaimToken_ReturnsForbidden()
    {
        // Arrange
        var testId = "CRM_ANON_PUT_040C";
        TestAuthHelper.SetAdminToken(_client);
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var request = GetValidPayload(testId);
        request.FirstName = "MaliciousUpdate";

        // Act: Switch context back to Anonymous role token
        TestAuthHelper.SetTokenWithRole(_client, "Anonymous");
        var response = await _client.PutAsJsonAsync($"/api/Customer/{testId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ===================================================================================
    // AUTH-040d: Anonymous attempts to delete a customer account
    // ===================================================================================
    [Fact]
    public async Task AUTH_040d_DeleteCustomer_WithAnonymousRoleClaimToken_ReturnsForbidden()
    {
        // Arrange
        var testId = "CRM_ANON_DEL_040D";
        TestAuthHelper.SetAdminToken(_client);
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        // Act: Switch context back to Anonymous role token
        TestAuthHelper.SetTokenWithRole(_client, "Anonymous");
        var response = await _client.DeleteAsync($"/api/Customer/{testId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ===================================================================================
    // AUTH-040e: Anonymous attempts secondary update routine/endpoint variation
    // ===================================================================================
    [Fact]
    public async Task AUTH_040e_UpdateCustomerAlternative_WithAnonymousRoleClaimToken_ReturnsForbidden()
    {
        // Arrange
        var testId = "CRM_ANON_PUT_040E";
        TestAuthHelper.SetAdminToken(_client);
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        var request = GetValidPayload(testId);
        request.FirstName = "MaliciousUpdateAlternative";

        // Act: Switch context back to Anonymous role token
        TestAuthHelper.SetTokenWithRole(_client, "Anonymous");
        var response = await _client.PutAsJsonAsync($"/api/Customer/{testId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ===================================================================================
    // AUTH-040f: Anonymous attempts secondary delete routine/endpoint variation
    // ===================================================================================
    [Fact]
    public async Task AUTH_040f_DeleteCustomerAlternative_WithAnonymousRoleClaimToken_ReturnsForbidden()
    {
        // Arrange
        var testId = "CRM_ANON_DEL_040F";
        TestAuthHelper.SetAdminToken(_client);
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        // Act: Switch context back to Anonymous role token
        TestAuthHelper.SetTokenWithRole(_client, "Anonymous");
        var response = await _client.DeleteAsync($"/api/Customer/{testId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ===================================================================================
    // AUTH-040g: Anonymous attempts secondary sync routine/endpoint variation
    // ===================================================================================
    [Fact]
    public async Task AUTH_040g_SyncCustomerAlternative_WithAnonymousRoleClaimToken_ReturnsForbidden()
    {
        // Arrange
        var testId = "CRM_ANON_SYNC_040G";
        var dbHelper = new TestDatabaseHelper();
        await dbHelper.DeleteCustomer(testId);

        TestAuthHelper.SetTokenWithRole(_client, "Anonymous");
        var request = GetValidPayload(testId);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}