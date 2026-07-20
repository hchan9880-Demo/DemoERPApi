using DemoERPApi.Models;
using DemoERPApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static DemoERPApi.Tests.Helpers.TestData;

namespace DemoERPApi.Tests.Integration.Security;


/// Comprehensive security tests covering authentication, authorization,
/// input validation, and data ownership for the Customer API endpoints.
/// 
/// Test Cases Covered:
/// SEC-001    /api/Customer/{id}    GET    Missing Authorization header            401 Unauthorized
/// SEC-002    /api/Customer/{id}    GET    Invalid JWT token                        401 Unauthorized
/// SEC-003    /api/Customer/{id}    PUT    Missing Authorization header            401 Unauthorized
/// SEC-004    /api/Customer/{id}    PUT    Invalid JWT token                        401 Unauthorized
/// SEC-005    /api/Customer/{id}    DELETE Missing Authorization header            401 Unauthorized
/// SEC-006    /api/Customer/{id}    DELETE Invalid JWT token                        401 Unauthorized
/// SEC-007    /api/Customer/sync    POST   Missing Authorization header            401 Unauthorized
/// SEC-008    /api/Customer/sync    POST   Invalid JWT token                        401 Unauthorized
/// SEC-009    All Protected Endpoints Any   Expired JWT                            401 Unauthorized
/// SEC-010    All Protected Endpoints Any   Tampered JWT signature                 401 Unauthorized
/// SEC-011    All Protected Endpoints Any   Unsupported role claim                 403 Forbidden
/// SEC-012    Customer APIs           POST/PUT SQL Injection / XSS payload         400 Bad Request
/// SEC-013    All Endpoints           Any    No stack trace leakage                RFC7807 Problem Details
/// SEC-014    /api/Customer/{id}    GET    Customer retrieves own profile         200 OK
/// SEC-015    /api/Customer/{id}    PUT    Customer updates own profile           200 OK

public class SecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    // Test data constants
    private const string TargetCustomerId = "SEC_TEST_99";
    private const string OwnerUsername = "owner1";

    
    /// Initializes a new instance of the <see cref="SecurityTests"/> class.
    /// Resets the database and creates a clean test client before each test.
    
    public SecurityTests(
        WebApplicationFactory<Program> factory,
        ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;

        // Reset database to clean state
        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient();
    }

    #region Test Data Helpers

    
    /// Creates a valid customer payload for testing.
    
    /// <param name="customId">Optional custom ID, defaults to TargetCustomerId</param>
    /// <returns>Valid CustomersDto object</returns>
    private static CustomersDto GetValidPayload(string customId = TargetCustomerId) => new()
    {
        CRMCustomerID = customId,
        FirstName = "Security",
        LastName = "Check",
        Email = "security@test.com",
        Phone = "6041234567"
    };

    
    /// Creates a malicious customer payload for security testing.
    
    /// <param name="customId">Optional custom ID</param>
    /// <returns>CustomersDto with XSS and SQL injection payloads</returns>
    private static CustomersDto GetMaliciousPayload(string customId = TargetCustomerId) => new()
    {
        CRMCustomerID = customId,
        FirstName = "<script>alert('xss')</script>",
        LastName = "'; DROP TABLE Customers;--",
        Email = "malicious@test.com",
        Phone = "6041234567"
    };

    
    /// Sets up a customer with ownership for testing data ownership scenarios.
    
    /// <param name="customerId">Customer ID to create and assign ownership</param>
    private async Task SetupCustomerWithOwnership(string customerId)
    {
        // Create customer as Admin
        TestAuthHelper.SetAdminToken(_client);
        await CustomerSeedHelper.SeedCustomer(_client, customerId, _output);
        await CustomerSeedHelper.AssignCustomerAccess(customerId, OwnerUsername, _output);
        await CustomerSeedHelper.VerifyCustomerAccess(_factory.Services, OwnerUsername, customerId, _output);
    }

    
    /// Debug helper to output current JWT token claims.
    
    private void DebugCurrentToken()
    {
        var token = _client.DefaultRequestHeaders.Authorization?.Parameter;

        if (string.IsNullOrEmpty(token))
        {
            _output.WriteLine("No JWT token found.");
            return;
        }

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        _output.WriteLine("========== JWT CLAIMS ==========");
        foreach (var claim in jwt.Claims)
        {
            _output.WriteLine($"{claim.Type} = {claim.Value}");
        }
    }

    #endregion

    #region Route Specific Authentication Tests (SEC-001 to SEC-008)

    
    /// SEC-001: Tests that GET /api/Customer/{id} requires authentication.
    
    [Fact]
    public async Task SEC_001_GetCustomer_MissingAuthHeader_ReturnsUnauthorized()
    {
        // Arrange: Clear any existing auth token
        TestAuthHelper.ClearToken(_client);

        // Act: Attempt to get customer without auth
        var response = await _client.GetAsync($"/api/Customer/{TargetCustomerId}");

        // Assert: Should return 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    
    /// SEC-002: Tests that GET /api/Customer/{id} rejects invalid JWT tokens.
    
    [Fact]
    public async Task SEC_002_GetCustomer_InvalidJwtToken_ReturnsUnauthorized()
    {
        // Arrange: Set invalid token
        TestAuthHelper.SetInvalidToken(_client);

        // Act: Attempt to get customer with invalid token
        var response = await _client.GetAsync($"/api/Customer/{TargetCustomerId}");

        // Assert: Should return 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    
    /// SEC-003: Tests that PUT /api/Customer/{id} requires authentication.
    
    [Fact]
    public async Task SEC_003_UpdateCustomer_MissingAuthHeader_ReturnsUnauthorized()
    {
        // Arrange: Clear any existing auth token
        TestAuthHelper.ClearToken(_client);

        // Act: Attempt to update customer without auth
        var response = await _client.PutAsJsonAsync($"/api/Customer/{TargetCustomerId}", GetValidPayload());

        // Assert: Should return 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    
    /// SEC-004: Tests that PUT /api/Customer/{id} rejects invalid JWT tokens.
    
    [Fact]
    public async Task SEC_004_UpdateCustomer_InvalidJwtToken_ReturnsUnauthorized()
    {
        // Arrange: Set invalid token
        TestAuthHelper.SetInvalidToken(_client);

        // Act: Attempt to update customer with invalid token
        var response = await _client.PutAsJsonAsync($"/api/Customer/{TargetCustomerId}", GetValidPayload());

        // Assert: Should return 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    
    /// SEC-005: Tests that DELETE /api/Customer/{id} requires authentication.
    
    [Fact]
    public async Task SEC_005_DeleteCustomer_MissingAuthHeader_ReturnsUnauthorized()
    {
        // Arrange: Clear any existing auth token
        TestAuthHelper.ClearToken(_client);

        // Act: Attempt to delete customer without auth
        var response = await _client.DeleteAsync($"/api/Customer/{TargetCustomerId}");

        // Assert: Should return 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    
    /// SEC-006: Tests that DELETE /api/Customer/{id} rejects invalid JWT tokens.
    
    [Fact]
    public async Task SEC_006_DeleteCustomer_InvalidJwtToken_ReturnsUnauthorized()
    {
        // Arrange: Set invalid token
        TestAuthHelper.SetInvalidToken(_client);

        // Act: Attempt to delete customer with invalid token
        var response = await _client.DeleteAsync($"/api/Customer/{TargetCustomerId}");

        // Assert: Should return 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    
    /// SEC-007: Tests that POST /api/Customer/sync requires authentication.
    
    [Fact]
    public async Task SEC_007_SyncCustomer_MissingAuthHeader_ReturnsUnauthorized()
    {
        // Arrange: Clear any existing auth token
        TestAuthHelper.ClearToken(_client);

        // Act: Attempt to sync customer without auth
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", GetValidPayload());

        // Assert: Should return 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    
    /// SEC-008: Tests that POST /api/Customer/sync rejects invalid JWT tokens.
    
    [Fact]
    public async Task SEC_008_SyncCustomer_InvalidJwtToken_ReturnsUnauthorized()
    {
        // Arrange: Set invalid token
        TestAuthHelper.SetInvalidToken(_client);

        // Act: Attempt to sync customer with invalid token
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", GetValidPayload());

        // Assert: Should return 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region General Security Protocol Tests (SEC-009 to SEC-013)

    
    /// SEC-009: Tests that expired JWT tokens are rejected.
    
    [Fact]
    public async Task SEC_009_AnyEndpoint_ExpiredJwt_ReturnsUnauthorized()
    {
        // Arrange: Set expired token
        TestAuthHelper.SetExpiredToken(_client);

        // Act: Attempt API call with expired token
        var response = await _client.GetAsync($"/api/Customer/{TargetCustomerId}");

        // Assert: Should return 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    
    /// SEC-010: Tests that tampered JWT tokens are rejected.
    
    [Fact]
    public async Task SEC_010_AnyEndpoint_TamperedJwtSignature_ReturnsUnauthorized()
    {
        // Arrange: Set tampered token
        TestAuthHelper.SetTamperedToken(_client);

        // Act: Attempt API call with tampered token
        var response = await _client.GetAsync($"/api/Customer/{TargetCustomerId}");

        // Assert: Should return 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    
    /// SEC-011: Tests that unsupported role claims result in 403 Forbidden.
    
    [Fact]
    public async Task SEC_011_AnyEndpoint_UnsupportedRoleClaim_ReturnsForbidden()
    {
        // Arrange: Set token with unauthorized user role
        TestAuthHelper.SetUnauthorizedUserToken(_client);

        // Act: Attempt delete with unsupported role
        var response = await _client.DeleteAsync($"/api/Customer/{TargetCustomerId}");

        // Assert: Should return 403 Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    
    /// SEC-012: Tests that SQL injection and XSS payloads are rejected.
    
    [Fact]
    public async Task SEC_012_CustomerApis_SqlInjectionOrXssPayload_ReturnsBadRequest()
    {
        // Arrange: Set admin token and malicious payload
        TestAuthHelper.SetAdminToken(_client);
        var maliciousPayload = GetMaliciousPayload();

        // Act: Attempt sync with malicious payload
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", maliciousPayload);

        // Assert: Should return 400 BadRequest
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    
    /// SEC-013: Tests that error responses don't leak stack trace information.
    
    [Fact]
    public async Task SEC_013_AllEndpoints_OnError_NoStackTraceLeakage_ReturnsRFC7807ProblemDetails()
    {
        // Arrange: Set admin token and use invalid route
        TestAuthHelper.SetAdminToken(_client);

        // Act: Call invalid route to trigger error
        var response = await _client.GetAsync("/api/Customer/%%INVALID_ROUTE%%");
        var content = await response.Content.ReadAsStringAsync();

        // Assert: No stack trace information is exposed
        Assert.DoesNotContain("Exception:", content);
        Assert.DoesNotContain("at DemoERPApi.", content);

        // Assert: Response follows RFC7807 Problem Details format
        if (response.Content.Headers.ContentType?.MediaType == "application/problem+json")
        {
            using var doc = JsonDocument.Parse(content);
            Assert.True(doc.RootElement.TryGetProperty("status", out _));
            Assert.True(doc.RootElement.TryGetProperty("title", out _));
        }
    }

    #endregion

    #region Data Ownership Tests (SEC-014 to SEC-015)

    
    /// SEC-014: Tests that a customer can retrieve their own profile.
    
    [Fact]
    public async Task SEC_014_GetCustomer_CustomerRetrievesOwnCustomerRecord_ReturnsOk()
    {
        // Arrange: Create test customer with ownership
        var testId = "SEC_OWN_014";
        await SetupCustomerWithOwnership(testId);

        // Act: Switch to owner token and retrieve customer
        TestAuthHelper.SetOwnerToken(_client);
        DebugCurrentToken();

        var response = await _client.GetAsync($"/api/Customer/{testId}");

        // Assert: Should return 200 OK with customer data
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var returnedCustomer = await response.Content.ReadFromJsonAsync<CustomersDto>();
        Assert.NotNull(returnedCustomer);
        Assert.Equal(testId, returnedCustomer.CRMCustomerID);
    }

    
    /// SEC-015: Tests that a customer can update their own profile.
    
    [Fact]
    public async Task SEC_015_UpdateCustomer_CustomerUpdatesOwnProfile_ReturnsOk()
    {
        // Arrange: Create test customer with ownership
        var testId = "SEC_OWN_015";
        await SetupCustomerWithOwnership(testId);

        // Act: Switch to owner token and update customer
        TestAuthHelper.SetOwnerToken(_client);
        var updatePayload = GetValidPayload(testId);
        updatePayload.FirstName = "SecurityUpdated";

        var response = await _client.PutAsJsonAsync($"/api/Customer/{testId}", updatePayload);

        // Assert: Update successful
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act: Verify the update was persisted
        var verifyResponse = await _client.GetAsync($"/api/Customer/{testId}");
        var verifiedCustomer = await verifyResponse.Content.ReadFromJsonAsync<CustomersDto>();

        // Assert: Verify response status and data
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);
        Assert.NotNull(verifiedCustomer);
        Assert.Equal(testId, verifiedCustomer.CRMCustomerID);
        Assert.Equal("SecurityUpdated", verifiedCustomer.FirstName);
    }

    #endregion
}