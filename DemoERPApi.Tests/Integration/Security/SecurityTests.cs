using DemoERPApi.Models;
using DemoERPApi.Services;
using DemoERPApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.VisualStudio.TestPlatform.Utilities;
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
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    private const string TARGET_ID = "SEC_TEST_99";

    public SecurityTests(
        WebApplicationFactory<Program> factory,
        ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;

        DatabaseResetHelper.Reset(factory.Services);

        _client = factory.CreateClient();
    }

    private static CustomersDto GetValidPayload(string customId = TARGET_ID) => new()
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
    // =====================================================
    // SEC-001: Get customer without Authorization header
    //
    // Workflow:
    // JWT supplied?
    //      ↓ No
    // Authentication fails
    //      ↓
    // Return 401 Unauthorized
    // =====================================================
    [Fact]
    public async Task SEC_001_GetCustomer_MissingAuthHeader_ReturnsUnauthorized()
    {
        TestAuthHelper.ClearToken(_client);
        var response = await _client.GetAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    // =====================================================
    // SEC-002: Get customer with invalid JWT token
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
    public async Task SEC_002_GetCustomer_InvalidJwtToken_ReturnsUnauthorized()
    {
        TestAuthHelper.SetInvalidToken(_client);
        var response = await _client.GetAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    // =====================================================
    // SEC-003: Update customer without Authorization header
    //
    // Workflow:
    // JWT supplied?
    //      ↓ No
    // Authentication fails
    //      ↓
    // Return 401 Unauthorized
    // =====================================================
    [Fact]
    public async Task SEC_003_UpdateCustomer_MissingAuthHeader_ReturnsUnauthorized()
    {
        TestAuthHelper.ClearToken(_client);
        var response = await _client.PutAsJsonAsync($"/api/Customer/{TARGET_ID}", GetValidPayload());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    // =====================================================
    // SEC-004: Update customer with invalid JWT token
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
    public async Task SEC_004_UpdateCustomer_InvalidJwtToken_ReturnsUnauthorized()
    {
        TestAuthHelper.SetInvalidToken(_client);
        var response = await _client.PutAsJsonAsync($"/api/Customer/{TARGET_ID}", GetValidPayload());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    // =====================================================
    // SEC-005: Delete customer without Authorization header
    //
    // Workflow:
    // JWT supplied?
    //      ↓ No
    // Authentication fails
    //      ↓
    // Return 401 Unauthorized
    // =====================================================
    [Fact]
    public async Task SEC_005_DeleteCustomer_MissingAuthHeader_ReturnsUnauthorized()
    {
        TestAuthHelper.ClearToken(_client);
        var response = await _client.DeleteAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    // =====================================================
    // SEC-006: Delete customer with invalid JWT token
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
    public async Task SEC_006_DeleteCustomer_InvalidJwtToken_ReturnsUnauthorized()
    {
        TestAuthHelper.SetInvalidToken(_client);
        var response = await _client.DeleteAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    // =====================================================
    // SEC-007: Sync customer without Authorization header
    //
    // Workflow:
    // JWT supplied?
    //      ↓ No
    // Authentication fails
    //      ↓
    // Return 401 Unauthorized
    // =====================================================
    [Fact]
    public async Task SEC_007_SyncCustomer_MissingAuthHeader_ReturnsUnauthorized()
    {
        TestAuthHelper.ClearToken(_client);
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", GetValidPayload());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    // =====================================================
    // SEC-008: Sync customer with invalid JWT token
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
    public async Task SEC_008_SyncCustomer_InvalidJwtToken_ReturnsUnauthorized()
    {
        TestAuthHelper.SetInvalidToken(_client);
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", GetValidPayload());
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // =====================================================
    // GENERAL SECURITY PROTOCOLS (SEC-009 to SEC-013)
    // =====================================================
    // =====================================================
    // SEC-009: Expired JWT Token
    //
    // Workflow:
    // JWT supplied
    //      ↓
    // JWT expired
    //      ↓
    // Authentication fails
    //      ↓
    // Return 401 Unauthorized
    // =====================================================
    [Fact]
    public async Task SEC_009_AnyEndpoint_ExpiredJwt_ReturnsUnauthorized()
    {
        TestAuthHelper.SetExpiredToken(_client);
        var response = await _client.GetAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    // =====================================================
    // SEC-010: Tampered JWT Signature
    //
    // Workflow:
    // JWT supplied
    //      ↓
    // JWT signature invalid
    //      ↓
    // Authentication fails
    //      ↓
    // Return 401 Unauthorized
    // =====================================================
    [Fact]
    public async Task SEC_010_AnyEndpoint_TamperedJwtSignature_ReturnsUnauthorized()
    {
        TestAuthHelper.SetTamperedToken(_client);
        var response = await _client.GetAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    // =====================================================
    // SEC-011: Unsupported Role Claim
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role claim recognized?
    //      ↓ No
    // Authorization fails
    //      ↓
    // Return 403 Forbidden
    // =====================================================
    [Fact]
    public async Task SEC_011_AnyEndpoint_UnsupportedRoleClaim_ReturnsForbidden()
    {
        TestAuthHelper.SetUnauthorizedUserToken(_client); 
        var response = await _client.DeleteAsync($"/api/Customer/{TARGET_ID}");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    // =====================================================
    // SEC-012: SQL Injection / XSS Payload
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Admin
    //      ↓
    // Request payload valid?
    //      ↓ No
    // Input validation detects malicious content
    //      ↓
    // Return 400 BadRequest
    // =====================================================
    [Fact]
    public async Task SEC_012_CustomerApis_SqlInjectionOrXssPayload_ReturnsBadRequest()
    {
        TestAuthHelper.SetAdminToken(_client);
        var maliciousPayload = new CustomersDto
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
    // =====================================================
    // SEC-013: No Stack Trace Leakage
    //
    // Workflow:
    // Request triggers server error
    //      ↓
    // API generates RFC7807 Problem Details
    //      ↓
    // Internal exception details hidden
    //      ↓
    // Return sanitized error response
    // =====================================================
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


    // =====================================================
    // SEC-014: Customer Retrieves Own Profile
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
    public async Task SEC_014_GetCustomer_CustomerRetrievesOwnCustomerRecord_ReturnsOk()
    {
        var testId = "SEC_OWN_014";

        //
        // Step 1:
        // Use Admin role to create customer
        //
        TestAuthHelper.SetAdminToken(_client);

        await CustomerSeedHelper.SeedCustomer(
            _client,
            testId,
            _output);


        //
        // Step 2:
        // Assign ownership:
        //
        // CustomerAccess:
        // SEC_OWN_014 -> owner1
        //
        await CustomerSeedHelper.AssignCustomerAccess(
            testId,
            "owner1",
            _output);


        //
        // Step 3:
        // Verify database relationship exists
        //
        await CustomerSeedHelper.VerifyCustomerAccess(
            _factory.Services,
            "owner1",
            testId,
            _output);



        //
        // Step 4:
        // Switch JWT identity to customer owner
        //
        TestAuthHelper.SetOwnerToken(_client);


        DebugCurrentToken();


        //
        // Step 5:
        // Customer retrieves own record
        //
        var response =
            await _client.GetAsync(
                $"/api/Customer/{testId}");



        //
        // Step 6:
        // Assert authorization succeeded
        //
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);



        var returnedCustomer =
            await response.Content.ReadFromJsonAsync<CustomersDto>();


        Assert.NotNull(returnedCustomer);


        Assert.Equal(
            testId,
            returnedCustomer.CRMCustomerID);
    }

    // =====================================================
    // SEC-015: Customer Updates Own Profile
    //
    // Workflow:
    // JWT valid
    //      ↓
    // Role = Customer
    //      ↓
    // Requested customer is owner?
    //      ↓ Yes
    // Update payload valid?
    //      ↓ Yes
    // Update customer
    //      ↓
    // Verify updated values
    //      ↓
    // Return 200 OK
    // =====================================================

    [Fact]
    public async Task SEC_015_UpdateCustomer_CustomerUpdatesOwnProfile_ReturnsOk()
    {
        var testId = "SEC_OWN_015";

        // 1. Setup Data as Admin
        TestAuthHelper.SetAdminToken(_client);
        await CustomerSeedHelper.SeedCustomer(_client, testId, _output);
        await CustomerSeedHelper.AssignCustomerAccess(testId, "owner1", _output);

        // 2. Switch to Owner and Update
        TestAuthHelper.SetOwnerToken(_client);
        var updatePayload = GetValidPayload(testId);
        updatePayload.FirstName = "SecurityUpdated";

        var response = await _client.PutAsJsonAsync($"/api/Customer/{testId}", updatePayload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // 3. Verify Persistence
        var verifyResponse = await _client.GetAsync($"/api/Customer/{testId}");
        var verifiedCustomer = await verifyResponse.Content.ReadFromJsonAsync<CustomersDto>();


        //
        // Step 6:
        // Verify update succeeded
        //
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);





        Assert.Equal(
            HttpStatusCode.OK,
            verifyResponse.StatusCode);




        Assert.NotNull(verifiedCustomer);


        Assert.Equal(
            testId,
            verifiedCustomer.CRMCustomerID);



        Assert.Equal(
            "SecurityUpdated",
            verifiedCustomer.FirstName);

        Assert.Equal("SecurityUpdated", verifiedCustomer?.FirstName);

    }












    private void DebugCurrentToken()
    {
        var token =
            _client.DefaultRequestHeaders.Authorization?.Parameter;

        if (string.IsNullOrEmpty(token))
        {
            _output.WriteLine("No JWT token found.");
            return;
        }


        var jwt =
            new JwtSecurityTokenHandler()
                .ReadJwtToken(token);


        _output.WriteLine(
            "========== JWT CLAIMS ==========");


        foreach (var claim in jwt.Claims)
        {
            _output.WriteLine(
                $"{claim.Type} = {claim.Value}");
        }
    }
}