using DemoERPApi.Models;
using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Xunit;

namespace DemoERPApi.Tests.Integration.Logging;

/*
 * 
LoggingMiddlewareTests
      |
      v
CustomWebApplicationFactory
      |
      v
CreateClient()
      |
      v
EF Core creates DbContext
      |
      v
EF validates all entities
      |
      v
SyncLog has no primary key
      |
      v
Application startup fails


 * 
LOG-001 Customer successfully created
Expected:
• HTTP 200 OK
• Information log written
• Customer creation completed

LOG-002 Duplicate customer submitted
Expected:
• HTTP 409 Conflict
• Warning log written

LOG-003 Invalid customer payload
Expected:
• HTTP 400 BadRequest
• Warning log written
*/

public class LoggingMiddlewareTests :
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient Client;

    public LoggingMiddlewareTests(CustomWebApplicationFactory factory)
    {
        Client = factory.CreateClient();

        TestAuthHelper.SetAdminToken(Client);
    }

    [Fact(DisplayName = "LOG-001 Customer successfully created")]
    public async Task LOG_001_CustomerSuccessfullyCreated_ReturnsOk()
    {
        var request = new CustomersDto
        {
            CRMCustomerID = "LOG001_TEST_999996",
            FirstName = "ABC",
            LastName = "Company",
            Email = "abc@test.com",
            Phone = "6041111111"
        };

        var response =
            await Client.PostAsJsonAsync(
                "/api/Customer/sync",
                request);

        var body =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"Status: {response.StatusCode}");

        Console.WriteLine(
            $"Response: {body}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);



        // TODO:
        // Verify Information log written
    }

    [Fact(DisplayName = "LOG-002 Duplicate customer submitted")]
    public async Task LOG_002_DuplicateCustomer_ReturnsConflict()
    {
        var request = new CustomersDto
        {
            CRMCustomerID = "LOG002",
            FirstName = "Duplicate",
            LastName = "Customer",
            Email = "duplicate@test.com",
            Phone = "6042222222"
        };

        await Client.PostAsJsonAsync(
            "/api/Customer/sync",
            request);

        var response =
            await Client.PostAsJsonAsync(
                "/api/Customer/sync",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);

        // TODO:
        // Verify Warning log written
    }

    [Fact(DisplayName = "LOG-003 Invalid customer payload")]
    public async Task LOG_003_InvalidPayload_ReturnsBadRequest()
    {
        var request = new CustomersDto
        {
            CRMCustomerID = "",
            FirstName = "",
            LastName = "",
            Email = "invalid-email",
            Phone = ""
        };

        var response =
            await Client.PostAsJsonAsync(
                "/api/Customer/sync",
                request);

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);

        // TODO:
        // Verify Warning log written
    }
}