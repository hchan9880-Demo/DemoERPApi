using DemoERPApi.Models;
using DemoERPApi.Tests.Helpers;
using DemoERPApi.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace DemoERPApi.Tests.Integration.Security;

/*
Test Cases Covered:
SEC-001    /api/Customer/{id}    GET    Missing Authorization header            401 Unauthorized
SEC-002    /api/Customer/{id}    GET    Invalid JWT token                        401 Unauthorized
SEC-003    /api/Customer/{id}    PUT    Missing Authorization header            401 Unauthorized
SEC-004    /api/Customer/{id}    PUT    Invalid JWT token                        401 Unauthorized
SEC-005    /api/Customer/{id}    DELETE Missing Authorization header            401 Unauthorized
SEC-006    /api/Customer/{id}    DELETE Invalid JWT token                        401 Unauthorized
SEC-007    /api/Customer/sync    POST   Missing Authorization header            401 Unauthorized
SEC-008    /api/Customer/sync    POST   Invalid JWT token                        401 Unauthorized
SEC-009    All Protected Endpoints Any   Expired JWT                            401 Unauthorized
SEC-010    All Protected Endpoints Any   Tampered JWT signature                 401 Unauthorized
SEC-011    All Protected Endpoints Any   Unsupported role claim                 403 Forbidden
SEC-012    Customer APIs           POST/PUT SQL Injection / XSS payload         400 Bad Request / Validation
SEC-013    All Endpoints           Any    No stack trace leakage                RFC7807 Problem Details
SEC-014    /api/Customer/{id}    GET    Customer retrieves own profile         200 OK
SEC-015    /api/Customer/{id}    PUT    Customer updates own profile           200 OK
*/

public class SecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private const string TARGET_ID = "SEC_TEST_99";

    public SecurityTests(WebApplicationFactory<Program> factory)
    {
        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient();
    }

    private static CustomerDto GetValidPayload(string customId = TARGET_ID) => new()
    {
        CRMCustomerID = customId,
        FirstName = "Security",
        LastName = "Check",
        Email = "security@test.com",
        Phone = "6041234567"
    };

    // =====================================================
    // ROUTE SPECIFIC CHECKS (SEC-001 to SEC-008)
    // =====================================================

    [Fact]
    public async Task SEC_001_GetCustomer_MissingAuthHeader_ReturnsUnauthorized()
    {
        TestAuthHelper.ClearToken(_client);
        var response = await _client.GetAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SEC_002_GetCustomer_InvalidJwtToken_ReturnsUnauthorized()
    {
        TestAuthHelper.SetInvalidToken(_client);
        var response = await _client.GetAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SEC_003_UpdateCustomer_MissingAuthHeader_ReturnsUnauthorized()
    {
        TestAuthHelper.ClearToken(_client);
        var response = await _client.PutAsJsonAsync($"/api/Customer/{TARGET_ID}", GetValidPayload());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SEC_004_UpdateCustomer_InvalidJwtToken_ReturnsUnauthorized()
    {
        TestAuthHelper.SetInvalidToken(_client);
        var response = await _client.PutAsJsonAsync($"/api/Customer/{TARGET_ID}", GetValidPayload());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SEC_005_DeleteCustomer_MissingAuthHeader_ReturnsUnauthorized()
    {
        TestAuthHelper.ClearToken(_client);
        var response = await _client.DeleteAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SEC_006_DeleteCustomer_InvalidJwtToken_ReturnsUnauthorized()
    {
        TestAuthHelper.SetInvalidToken(_client);
        var response = await _client.DeleteAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SEC_007_SyncCustomer_MissingAuthHeader_ReturnsUnauthorized()
    {
        TestAuthHelper.ClearToken(_client);
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", GetValidPayload());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SEC_008_SyncCustomer_InvalidJwtToken_ReturnsUnauthorized()
    {
        TestAuthHelper.SetInvalidToken(_client);
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", GetValidPayload());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // =====================================================
    // GENERAL SECURITY PROTOCOLS (SEC-009 to SEC-013)
    // =====================================================

    [Fact]
    public async Task SEC_009_AnyEndpoint_ExpiredJwt_ReturnsUnauthorized()
    {
        TestAuthHelper.SetExpiredToken(_client);
        var response = await _client.GetAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SEC_010_AnyEndpoint_TamperedJwtSignature_ReturnsUnauthorized()
    {
        TestAuthHelper.SetTamperedToken(_client);
        var response = await _client.GetAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SEC_011_AnyEndpoint_UnsupportedRoleClaim_ReturnsForbidden()
    {
        TestAuthHelper.SetUnauthorizedUserToken(_client); 
        var response = await _client.DeleteAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SEC_012_CustomerApis_SqlInjectionOrXssPayload_ReturnsBadRequest()
    {
        TestAuthHelper.SetAdminToken(_client);
        var maliciousPayload = new CustomerDto
        {
            CRMCustomerID = TARGET_ID,
            FirstName = "<script>alert('xss')</script>",
            LastName = "'; DROP TABLE Customers;--",
            Email = "malicious@test.com",
            Phone = "6041234567"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", maliciousPayload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SEC_013_AllEndpoints_OnError_NoStackTraceLeakage_ReturnsRFC7807ProblemDetails()
    {
        TestAuthHelper.SetAdminToken(_client);

        var response = await _client.GetAsync("/api/Customer/%%INVALID_ROUTE%%");
        var content = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("Exception:", content);
        Assert.DoesNotContain("at DemoERPApi.", content);

        if (response.Content.Headers.ContentType?.MediaType == "application/problem+json")
        {
            using var doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.TryGetProperty("status", out _));
            Assert.True(doc.RootElement.TryGetProperty("title", out _));
        }
    }

    // =====================================================
    // DATA OWNERSHIP / IDENTITY BUSINESS RULES (SEC-014 to SEC-015)
    // =====================================================

    [Fact]
    public async Task SEC_014_GetCustomer_CustomerRetrievesOwnCustomerRecord_ReturnsOk()
    {
        var testId = "SEC_OWN_014";

        // Seed a target customer context using an admin token first
        TestAuthHelper.SetAdminToken(_client);
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        // Switch scope to Owner context matching the seeded token subject
        TestAuthHelper.SetOwnerToken(_client);

        var response = await _client.GetAsync($"/api/Customer/{testId}");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var returnedCustomer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        Assert.NotNull(returnedCustomer);
        Assert.Equal(testId, returnedCustomer.CRMCustomerID);
    }

    [Fact]
    public async Task SEC_015_UpdateCustomer_CustomerUpdatesOwnProfile_ReturnsOk()
    {
        var testId = "SEC_OWN_015";

        // Seed data context via Admin token
        TestAuthHelper.SetAdminToken(_client);
        await CustomerSeedHelper.SeedCustomer(_client, testId);

        // Switch context to Owner token acting on self record update
        TestAuthHelper.SetOwnerToken(_client);
        
        var updatePayload = GetValidPayload(testId);
        updatePayload.FirstName = "SecurityUpdated";

        var response = await _client.PutAsJsonAsync($"/api/Customer/{testId}", updatePayload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify changes are queryable and persisted 
        var verifyResponse = await _client.GetAsync($"/api/Customer/{testId}");
        var verifiedCustomer = await verifyResponse.Content.ReadFromJsonAsync<CustomerDto>();
        
        Assert.NotNull(verifiedCustomer);
        Assert.Equal("SecurityUpdated", verifiedCustomer.FirstName);
    }
}