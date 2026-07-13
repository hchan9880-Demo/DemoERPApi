using DemoERPApi.Models;
using DemoERPApi.Tests.Helpers;

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace DemoERPApi.Tests.Integration.Authentication;
using DemoERPApi.Tests.Helpers;
/*
Test Cases Covered:
AUTH-001	Valid login - admin	                  Valid username/password for admin
AUTH-002	Valid login - QA	                    Valid username/password for QA
AUTH-003	Valid login - customer	              Valid username/password for customer
AUTH-004	Invalid username	                    Unknown username + valid password
AUTH-005	Invalid password	                    Valid username + wrong password
AUTH-006	Missing username	                    {"password":"Password123"}
AUTH-007	Missing password	                    {"username":"admin"}
AUTH-008	Empty JSON body	                      {}
AUTH-009	Malformed JSON	                      {"username":"admin", "password": }
AUTH-010	Wrong content type / invalid payload  Non-JSON payload or wrong schema

Authentication pipeline (Login endpoint)

Login Request
    ↓
Model Validation
    ↓
User Lookup
    ↓
Password Verification
    ↓
Generate JWT
    ↓
200 OK

Authorization pipeline (Protected endpoints)

JWT Valid
    ↓
Role
    ↓
Authorization Policy
    ↓
Resource Access
    ↓
Business Operation
    ↓
200 OK / 403 Forbidden

 */

public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient();
    }

    // =====================================================
    // AUTH-001
    // Valid login - Admin
    //
    // Authentication Workflow:
    //
    // Login Request
    //      ↓
    // Validate Request Model
    //      ↓
    // Lookup User
    //      ↓
    // Verify Password
    //      ↓
    // Credentials Valid?
    //      ↓
    // Yes
    //      ↓
    // Generate JWT
    //      ↓
    // Return 200 OK
    // =====================================================
    [Fact]
    public async Task AUTH_001_ValidLoginAdmin_ReturnsOk()
    {
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "Password123"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // =====================================================
    // AUTH-002
    // Valid login - QA
    //
    // Authentication Workflow:
    //
    // Login Request
    //      ↓
    // Validate Request Model
    //      ↓
    // Lookup User
    //      ↓
    // Verify Password
    //      ↓
    // Credentials Valid?
    //      ↓
    // Yes
    //      ↓
    // Generate JWT
    //      ↓
    // Return 200 OK
    // =====================================================
    [Fact]
    public async Task AUTH_002_ValidLoginQA_ReturnsOk()
    {
        var request = new LoginRequest
        {
            Username = "qa_user",
            Password = "Password123"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // =====================================================
    // AUTH-003
    // Valid login - Customer
    //
    // Authentication Workflow:
    //
    // Login Request
    //      ↓
    // Validate Request Model
    //      ↓
    // Lookup User
    //      ↓
    // Verify Password
    //      ↓
    // Credentials Valid?
    //      ↓
    // Yes
    //      ↓
    // Generate JWT
    //      ↓
    // Return 200 OK
    // =====================================================
    [Fact]
    public async Task AUTH_003_ValidLoginCustomer_ReturnsOk()
    {
        var request = new LoginRequest
        {
            Username = "customer_user",
            Password = "Password123"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // =====================================================
    // AUTH-004
    // Invalid username
    //
    // Authentication Workflow:
    //
    // Login Request
    //      ↓
    // Validate Request Model
    //      ↓
    // Lookup User
    //      ↓
    // User Found?
    //      ↓
    // No
    //      ↓
    // Authentication Failed
    //      ↓
    // Return 401 Unauthorized
    // =====================================================
    [Fact]
    public async Task AUTH_004_InvalidUsername_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Username = "notreal",
            Password = "Password123"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // =====================================================
    // AUTH-005
    // Invalid password
    //
    // Authentication Workflow:
    //
    // Login Request
    //      ↓
    // Validate Request Model
    //      ↓
    // Lookup User
    //      ↓
    // Verify Password
    //      ↓
    // Password Correct?
    //      ↓
    // No
    //      ↓
    // Authentication Failed
    //      ↓
    // Return 401 Unauthorized
    // =====================================================
    [Fact]
    public async Task AUTH_005_InvalidPassword_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "WrongPassword"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // =====================================================
    // AUTH-006
    // Missing username
    //
    // Authentication Workflow:
    //
    // Login Request
    //      ↓
    // Model Validation
    //      ↓
    // Username Missing?
    //      ↓
    // Yes
    //      ↓
    // Return 400 Bad Request
    // =====================================================
    [Fact]
    public async Task AUTH_006_MissingUsername_ReturnsBadRequest()
    {
        var request = new LoginRequest
        {
            Username = null,
            Password = "Password123"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =====================================================
    // AUTH-007
    // Missing password
    //
    // Authentication Workflow:
    //
    // Login Request
    //      ↓
    // Model Validation
    //      ↓
    // Password Missing?
    //      ↓
    // Yes
    //      ↓
    // Return 400 Bad Request
    // =====================================================
    [Fact]
    public async Task AUTH_007_MissingPassword_ReturnsBadRequest()
    {
        var request = new LoginRequest
        {
            Username = "admin",
            Password = null
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =====================================================
    // AUTH-008
    // Empty JSON body
    //
    // Authentication Workflow:
    //
    // HTTP Request
    //      ↓
    // JSON Model Binding
    //      ↓
    // Required Fields Present?
    //      ↓
    // No
    //      ↓
    // Model Validation Failed
    //      ↓
    // Return 400 Bad Request
    // =====================================================
    [Fact]
    public async Task AUTH_008_EmptyJsonBody_ReturnsBadRequest()
    {
        var content = new StringContent("{}", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Auth/login", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =====================================================
    // AUTH-009
    // Malformed JSON
    //
    // Authentication Workflow:
    //
    // HTTP Request
    //      ↓
    // JSON Parsing
    //      ↓
    // Valid JSON?
    //      ↓
    // No
    //      ↓
    // Request Rejected
    //      ↓
    // Return 400 Bad Request
    // =====================================================
    [Fact]
    public async Task AUTH_009_MalformedJson_ReturnsBadRequest()
    {
        var malformedJson = "{\"username\":\"admin\", \"password\": }";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Auth/login", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =====================================================
    // AUTH-010
    // Wrong content type / invalid payload
    //
    // Authentication Workflow:
    //
    // HTTP Request
    //      ↓
    // Content-Type Validation
    //      ↓
    // Supported Media Type?
    //      ↓
    // No
    //      ↓
    // Request Rejected
    //      ↓
    // Return 415 Unsupported Media Type
    //           or
    // Return 400 Bad Request
    // =====================================================
    [Fact]
    public async Task AUTH_010_WrongContentTypeOrInvalidPayload_ReturnsUnsupportedMediaTypeOrBadRequest()
    {
        var plainTextContent = "username=admin&password=Password123";
        var content = new StringContent(plainTextContent, Encoding.UTF8, "text/plain");

        var response = await _client.PostAsync("/api/Auth/login", content);

        // API controllers using [ApiController] reject unmappable or mismatched media types with either 415 or 400
        Assert.True(response.StatusCode == HttpStatusCode.UnsupportedMediaType || response.StatusCode == HttpStatusCode.BadRequest);
    }
}