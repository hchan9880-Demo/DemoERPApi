using DemoERPApi.Models;
using DemoERPApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace DemoERPApi.Tests.Integration.Authorization;
/*
Test Cases Covered:
AUTH-038    /api/Customer/sync    POST      Role claim is null          -> 403 Forbidden
AUTH-042    /api/Customer/sync    POST      Role value is 'Manager'     -> 403 Forbidden
AUTH-051    /api/Customer/{id}    GET       Role claim is null/missing  -> 403 Forbidden
AUTH-052    /api/Customer/{id}    GET       Role value is empty string  -> 403 Forbidden
AUTH-053    /api/Customer/{id}    GET       Role value is 'Anonymous'   -> 403 Forbidden
AUTH-054    /api/Customer/{id}    GET       Role value is unsupported   -> 403 Forbidden
AUTH-055    /api/Customer/{id}    PUT       Role claim is null/missing  -> 403 Forbidden
AUTH-056    /api/Customer/{id}    PUT       Role value is empty string  -> 403 Forbidden
AUTH-057    /api/Customer/{id}    PUT       Role value is 'Anonymous'   -> 403 Forbidden
AUTH-058    /api/Customer/{id}    PUT       Role value is unsupported   -> 403 Forbidden
AUTH-059    /api/Customer/{id}    DELETE    Role claim is null/missing  -> 403 Forbidden
AUTH-060    /api/Customer/{id}    DELETE    Role value is empty string  -> 403 Forbidden
AUTH-061    /api/Customer/{id}    DELETE    Role value is 'Anonymous'   -> 403 Forbidden
AUTH-062    /api/Customer/{id}    DELETE    Role value is unsupported   -> 403 Forbidden
AUTH-063    /api/Customer/sync    POST      Role claim is null/missing  -> 403 Forbidden
AUTH-064    /api/Customer/sync    POST      Role value is empty string  -> 403 Forbidden
AUTH-065    /api/Customer/sync    POST      Role value is 'Anonymous'   -> 403 Forbidden
AUTH-066    /api/Customer/sync    POST      Role value is unsupported   -> 403 Forbidden
*/
/*
Authorization Flow 

JWT Valid
    ↓
Role Claim = Anonymous
    ↓
JWT Authentication
    ↓
Authorization Policy
    ↓
Allowed Roles?
    ├── Admin      → Yes
    ├── QA         → Yes
    ├── Customer   → Yes
    └── Anonymous  → No
                     ↓
Authorization Fails
    ↓
Controller Action NOT Executed
    ↓
HTTP 403 Forbidden

This test class verifies that an authenticated JWT containing an
unsupported role ("Anonymous") is rejected before any business logic
(GET, PUT, DELETE, or POST /sync) is executed.
*/
public class UnsupportedRoleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private const string TARGET_ID = "CRM_UNSUPPORTED_ROLE_TEST";

    public UnsupportedRoleTests(WebApplicationFactory<Program> factory)
    {
        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient();
    }

    private static CustomersDto GetValidPayload() => new()
    {
        CRMCustomerID = TARGET_ID,
        FirstName = "Security",
        LastName = "Restriction",
        Email = "unsupported_role@test.com",
        Phone = "6045550000"
    };

    // ===================================================================================
    // GET ENDPOINT TESTS (/api/Customer/{id})
    // ===================================================================================
    // =====================================================
    // AUTH-051
    // GET customer with null or missing role claim
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role Claim = null / missing
    //      ↓
    // Authorization Policy
    //      ↓
    // Valid Role?
    //      ↓
    // No
    //      ↓
    // Controller action is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-051
    public async Task AUTH_051_GetCustomer_NullOrMissingRoleClaim_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, null!);
        var response = await _client.GetAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // AUTH-052
    // GET customer with empty role value
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role Claim = ""
    //      ↓
    // Authorization Policy
    //      ↓
    // Valid Role?
    //      ↓
    // No
    //      ↓
    // Controller action is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-052
    public async Task AUTH_052_GetCustomer_EmptyRoleValue_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, "");
        var response = await _client.GetAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // AUTH-053
    // GET customer with Anonymous role
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role = Anonymous
    //      ↓
    // Authorization Policy
    //      ↓
    // Supported Role?
    //      ↓
    // No
    //      ↓
    // Controller action is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-053
    public async Task AUTH_053_GetCustomer_AnonymousRoleValue_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, "Anonymous");
        var response = await _client.GetAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // AUTH-054
    // GET customer with unsupported role
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role = Manager
    //      ↓
    // Authorization Policy
    //      ↓
    // Supported Role?
    //      ↓
    // No
    //      ↓
    // Controller action is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-054
    public async Task AUTH_054_GetCustomer_UnsupportedRoleValue_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, "Manager");
        var response = await _client.GetAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ===================================================================================
    // PUT ENDPOINT TESTS (/api/Customer/{id})
    // ===================================================================================
    // =====================================================
    // AUTH-055
    // PUT customer with null or missing role claim
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role Claim = null / missing
    //      ↓
    // Authorization Policy
    //      ↓
    // Valid Role?
    //      ↓
    // No
    //      ↓
    // Update operation is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-055
    public async Task AUTH_055_UpdateCustomer_NullOrMissingRoleClaim_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, null!);
        var response = await _client.PutAsJsonAsync($"/api/Customer/{TARGET_ID}", GetValidPayload());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // AUTH-056
    // PUT customer with empty role value
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role Claim = ""
    //      ↓
    // Authorization Policy
    //      ↓
    // Valid Role?
    //      ↓
    // No
    //      ↓
    // Update operation is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-056
    public async Task AUTH_056_UpdateCustomer_EmptyRoleValue_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, "");
        var response = await _client.PutAsJsonAsync($"/api/Customer/{TARGET_ID}", GetValidPayload());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // AUTH-057
    // PUT customer with Anonymous role
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role = Anonymous
    //      ↓
    // Authorization Policy
    //      ↓
    // Supported Role?
    //      ↓
    // No
    //      ↓
    // Update operation is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-057
    public async Task AUTH_057_UpdateCustomer_AnonymousRoleValue_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, "Anonymous");
        var response = await _client.PutAsJsonAsync($"/api/Customer/{TARGET_ID}", GetValidPayload());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // AUTH-058
    // PUT customer with unsupported role
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role = Guest
    //      ↓
    // Authorization Policy
    //      ↓
    // Supported Role?
    //      ↓
    // No
    //      ↓
    // Update operation is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-058
    public async Task AUTH_058_UpdateCustomer_UnsupportedRoleValue_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, "Guest");
        var response = await _client.PutAsJsonAsync($"/api/Customer/{TARGET_ID}", GetValidPayload());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ===================================================================================
    // DELETE ENDPOINT TESTS (/api/Customer/{id})
    // ===================================================================================
    // =====================================================
    // AUTH-059
    // DELETE customer with null or missing role claim
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role Claim = null / missing
    //      ↓
    // Authorization Policy
    //      ↓
    // Valid Role?
    //      ↓
    // No
    //      ↓
    // Delete operation is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-059
    public async Task AUTH_059_DeleteCustomer_NullOrMissingRoleClaim_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, null!);
        var response = await _client.DeleteAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // AUTH-060
    // DELETE customer with empty role value
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role Claim = ""
    //      ↓
    // Authorization Policy
    //      ↓
    // Valid Role?
    //      ↓
    // No
    //      ↓
    // Delete operation is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-060
    public async Task AUTH_060_DeleteCustomer_EmptyRoleValue_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, "");
        var response = await _client.DeleteAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // AUTH-061
    // DELETE customer with Anonymous role
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role = Anonymous
    //      ↓
    // Authorization Policy
    //      ↓
    // Supported Role?
    //      ↓
    // No
    //      ↓
    // Delete operation is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-061
    public async Task AUTH_061_DeleteCustomer_AnonymousRoleValue_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, "Anonymous");
        var response = await _client.DeleteAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // AUTH-062
    // DELETE customer with unsupported role
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role = Test
    //      ↓
    // Authorization Policy
    //      ↓
    // Supported Role?
    //      ↓
    // No
    //      ↓
    // Delete operation is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-062
    public async Task AUTH_062_DeleteCustomer_UnsupportedRoleValue_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, "Test");
        var response = await _client.DeleteAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ===================================================================================
    // POST SYNC ENDPOINT TESTS (/api/Customer/sync)
    // ===================================================================================
    // =====================================================
    // AUTH-038
    // POST sync customer with null role claim
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role Claim = null
    //      ↓
    // Authorization Policy
    //      ↓
    // Valid Role?
    //      ↓
    // No
    //      ↓
    // Sync operation is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-038
    public async Task AUTH_038_SyncCustomer_NullRoleClaim_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, null!);
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", GetValidPayload());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // AUTH-042
    // POST sync customer with Manager role
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role = Manager
    //      ↓
    // Authorization Policy
    //      ↓
    // Supported Role?
    //      ↓
    // No
    //      ↓
    // Sync operation is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-042
    public async Task AUTH_042_SyncCustomer_ManagerRoleValue_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, "Manager");
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", GetValidPayload());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // AUTH-063
    // POST sync customer with missing role claim
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role Claim = missing
    //      ↓
    // Authorization Policy
    //      ↓
    // Valid Role?
    //      ↓
    // No
    //      ↓
    // Sync operation is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-063
    public async Task AUTH_063_SyncCustomer_MissingRoleClaim_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, null!);
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", GetValidPayload());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // AUTH-064
    // POST sync customer with empty role value
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role Claim = ""
    //      ↓
    // Authorization Policy
    //      ↓
    // Valid Role?
    //      ↓
    // No
    //      ↓
    // Sync operation is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-064
    public async Task AUTH_064_SyncCustomer_EmptyRoleValue_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, "");
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", GetValidPayload());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // AUTH-065
    // POST sync customer with Anonymous role
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role = Anonymous
    //      ↓
    // Authorization Policy
    //      ↓
    // Supported Role?
    //      ↓
    // No
    //      ↓
    // Sync operation is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-065
    public async Task AUTH_065_SyncCustomer_AnonymousRoleValue_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, "Anonymous");
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", GetValidPayload());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // AUTH-066
    // POST sync customer with unsupported role
    //
    // Authorization Workflow:
    //
    // JWT Valid
    //      ↓
    // Role = Guest
    //      ↓
    // Authorization Policy
    //      ↓
    // Supported Role?
    //      ↓
    // No
    //      ↓
    // Sync operation is NOT executed
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact] // AUTH-066
    public async Task AUTH_066_SyncCustomer_UnsupportedRoleValue_ReturnsForbidden()
    {
        TestAuthHelper.SetTokenWithRole(_client, "Guest");
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", GetValidPayload());
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}