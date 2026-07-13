using DemoERPApi.Models;
using DemoERPApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DemoERPApi.Tests.Integration.Validation;

/*
Test Cases Covered:
VALID-001	Malformed JSON update payload       Invalid JSON body
VALID-002	Missing required fields on update   Missing required customer fields
VALID-003	Invalid email on update             Email="bad@"
VALID-004	Invalid phone on update             Phone invalid format
VALID-005	Malformed JSON sync payload         Invalid JSON body
VALID-006	Missing required fields on sync     Missing required customer fields
VALID-007	Invalid email on sync               Email="bad@"
VALID-008	Invalid phone on sync               Phone invalid format
VALID-009	Malformed JSON create payload       Invalid JSON body
VALID-010	Missing required fields on create   Missing required customer fields
VALID-011	Invalid email on create             Email="bad@"
VALID-012	Invalid phone on create             Phone invalid format
*/

public class CustomerValidationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private const string TARGET_ID = "CRM_VAL_99";

    public CustomerValidationTests(WebApplicationFactory<Program> factory)
    {
        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient();
    }

    // =====================================================
    // UPDATE ENDPOINT VALIDATION (PUT /api/Customer/{id})
    // =====================================================

    [Fact]
    public async Task VALID_001_UpdateMalformedJsonPayload_ReturnsBadRequest()
    {
        TestAuthHelper.SetAdminToken(_client);
        var malformedJson = "{ \"CRMCustomerID\": \"" + TARGET_ID + "\", \"FirstName\": ";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        var response = await _client.PutAsync($"/api/Customer/{TARGET_ID}", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VALID_002_UpdateMissingRequiredFields_ReturnsBadRequest()
    {
        TestAuthHelper.SetAdminToken(_client);
        var request = new CustomerDto
        {
            CRMCustomerID = TARGET_ID,
            FirstName = null, // Required
            LastName = "User",
            Email = "valid@mail.com",
            Phone = "6041234567"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{TARGET_ID}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VALID_003_UpdateInvalidEmail_ReturnsBadRequest()
    {
        TestAuthHelper.SetAdminToken(_client);
        var request = new CustomerDto
        {
            CRMCustomerID = TARGET_ID,
            FirstName = "Test",
            LastName = "User",
            Email = "bad@",
            Phone = "6041234567"
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{TARGET_ID}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VALID_004_UpdateInvalidPhone_ReturnsBadRequest()
    {
        TestAuthHelper.SetAdminToken(_client);
        var request = new CustomerDto
        {
            CRMCustomerID = TARGET_ID,
            FirstName = "Test",
            LastName = "User",
            Email = "valid@mail.com",
            Phone = "123" // Invalid format string
        };

        var response = await _client.PutAsJsonAsync($"/api/Customer/{TARGET_ID}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =====================================================
    // SYNC ENDPOINT VALIDATION (POST /api/Customer/sync)
    // =====================================================

    [Fact]
    public async Task VALID_005_SyncMalformedJsonPayload_ReturnsBadRequest()
    {
        TestAuthHelper.SetAdminToken(_client);
        var malformedJson = "{ \"CRMCustomerID\": \"" + TARGET_ID + "\", \"Email\": ";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Customer/sync", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VALID_006_SyncMissingRequiredFields_ReturnsBadRequest()
    {
        TestAuthHelper.SetAdminToken(_client);
        var request = new CustomerDto
        {
            CRMCustomerID = TARGET_ID,
            FirstName = "Test",
            LastName = null, // Required
            Email = "valid@mail.com",
            Phone = "6041234567"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VALID_007_SyncInvalidEmail_ReturnsBadRequest()
    {
        TestAuthHelper.SetAdminToken(_client);
        var request = new CustomerDto
        {
            CRMCustomerID = TARGET_ID,
            FirstName = "Test",
            LastName = "User",
            Email = "bad@",
            Phone = "6041234567"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VALID_008_SyncInvalidPhone_ReturnsBadRequest()
    {
        TestAuthHelper.SetAdminToken(_client);
        var request = new CustomerDto
        {
            CRMCustomerID = TARGET_ID,
            FirstName = "Test",
            LastName = "User",
            Email = "valid@mail.com",
            Phone = "abcdefg"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =====================================================
    // CREATE ENDPOINT VALIDATION (POST /api/Customer)
    // =====================================================

    [Fact]
    public async Task VALID_009_CreateMalformedJsonPayload_ReturnsBadRequest()
    {
        TestAuthHelper.SetAdminToken(_client);
        var malformedJson = "{ \"FirstName\": \"Test\", \"LastName\": ";
        var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Customer/sync", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VALID_010_CreateMissingRequiredFields_ReturnsBadRequest()
    {
        TestAuthHelper.SetAdminToken(_client);
        var request = new CustomerDto
        {
            CRMCustomerID = null, // Missing required ID key context
            FirstName = "Test",
            LastName = "User",
            Email = "valid@mail.com",
            Phone = "6041234567"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VALID_011_CreateInvalidEmail_ReturnsBadRequest()
    {
        TestAuthHelper.SetAdminToken(_client);
        var request = new CustomerDto
        {
            CRMCustomerID = TARGET_ID,
            FirstName = "Test",
            LastName = "User",
            Email = "bad@",
            Phone = "6041234567"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task VALID_012_CreateInvalidPhone_ReturnsBadRequest()
    {
        TestAuthHelper.SetAdminToken(_client);
        var request = new CustomerDto
        {
            CRMCustomerID = TARGET_ID,
            FirstName = "Test",
            LastName = "User",
            Email = "valid@mail.com",
            Phone = "000"
        };

        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}