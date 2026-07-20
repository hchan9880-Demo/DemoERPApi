using DemoERPApi.Models;
using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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


/// Integration tests for logging middleware and request/response logging.
/// Validates that the application properly logs successful operations,
/// duplicates, and validation errors with appropriate log levels.
/// 
/// Test Coverage:
/// LOG-001    Customer successfully created - HTTP 200 OK + Information log
/// LOG-002    Duplicate customer submitted - HTTP 409 Conflict + Warning log
/// LOG-003    Invalid customer payload - HTTP 400 BadRequest + Warning log

public class LoggingMiddlewareTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    // Test data constants
    private const string TestPhone = "6041234567";
    private const string TestEmail = "test@example.com";

    
    /// Initializes a new instance of the <see cref="LoggingMiddlewareTests"/> class.
    
    /// <param name="factory">Custom web application factory for testing</param>
    public LoggingMiddlewareTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        TestAuthHelper.SetAdminToken(_client);
    }

    #region Helper Methods

    
    /// Creates a valid customer request for testing.
    
    /// <param name="customerId">Customer CRM ID</param>
    /// <param name="firstName">First name</param>
    /// <param name="lastName">Last name</param>
    /// <param name="email">Email address</param>
    /// <param name="phone">Phone number</param>
    /// <returns>CustomersDto object</returns>
    private static CustomersDto CreateCustomerRequest(
        string customerId,
        string firstName = "Test",
        string lastName = "Customer",
        string email = TestEmail,
        string phone = TestPhone)
    {
        return new CustomersDto
        {
            CRMCustomerID = customerId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone
        };
    }

    
    /// Creates an invalid customer request for testing validation errors.
    
    /// <returns>Invalid CustomersDto object</returns>
    private static CustomersDto CreateInvalidCustomerRequest()
    {
        return new CustomersDto
        {
            CRMCustomerID = "",
            FirstName = "",
            LastName = "",
            Email = "invalid-email",
            Phone = ""
        };
    }

    #endregion

    #region Logging Tests


    /// LOG-001: Verifies that successfully creating a customer returns 200 OK
    /// and writes an Information level log.
    /// 
    /// Workflow:
    /// 1. Send a valid customer creation request
    /// 2. Verify the request succeeds with 200 OK
    /// 3. Verify an Information log is written (TODO: implement log verification)
    /// 
    /// This test ensures:
    /// - Successful operations are properly logged
    /// - Information level logs are generated for successful operations
    /// - The logging middleware captures successful requests
    /// - Customer creation completes successfully

    [Fact]
    //[Fact(DisplayName = "LOG-001 Customer successfully created returns OK")]
    public async Task LOG_001_CustomerSuccessfullyCreated_ReturnsOk()
    {
        // Arrange: Create a unique customer ID with timestamp
        var customerId = $"LOG001_TEST_{DateTime.UtcNow:yyyyMMddHHmmss}";
        var request = CreateCustomerRequest(
            customerId,
            firstName: "ABC",
            lastName: "Company",
            email: "abc@test.com",
            phone: "6041111111");

        // Act: Create customer
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        // Assert: Verify successful creation
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // TODO: Implement log verification
        // Verify Information log was written with:
        // - Log level: Information
        // - Message contains customer creation details
        // - Request ID is included
        // - User information is included
    }


    /// LOG-002: Verifies that submitting a duplicate customer returns 409 Conflict
    /// and writes a Warning level log.
    /// 
    /// Workflow:
    /// 1. Submit a valid customer creation request
    /// 2. Submit the same request again (duplicate)
    /// 3. Verify the second request returns 409 Conflict
    /// 4. Verify a Warning log is written (TODO: implement log verification)
    /// 
    /// This test ensures:
    /// - Duplicate detection works correctly
    /// - Conflict responses are properly logged
    /// - Warning level logs are generated for duplicates
    /// - The logging middleware captures business rule violations
    [Fact]
    //[Fact(DisplayName = "LOG-002 Duplicate customer returns Conflict")]
    public async Task LOG_002_DuplicateCustomer_ReturnsConflict()
    {
        // Arrange: Create customer request with fixed ID
        const string customerId = "LOG002";
        var request = CreateCustomerRequest(
            customerId,
            firstName: "Duplicate",
            lastName: "Customer",
            email: "duplicate@test.com",
            phone: "6042222222");

        // Act: First request - should succeed
        var firstResponse = await _client.PostAsJsonAsync("/api/Customer/sync", request);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Act: Second request - should fail as duplicate
        var secondResponse = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        // Assert: Verify duplicate detection
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // TODO: Implement log verification
        // Verify Warning log was written with:
        // - Log level: Warning
        // - Message indicates duplicate customer
        // - Conflict details are logged
        // - Request ID is included
    }


    /// LOG-003: Verifies that invalid customer payloads return 400 BadRequest
    /// and writes a Warning level log.
    /// 
    /// Workflow:
    /// 1. Submit an invalid customer request (empty ID, invalid email, etc.)
    /// 2. Verify the request returns 400 BadRequest
    /// 3. Verify a Warning log is written (TODO: implement log verification)
    /// 
    /// This test ensures:
    /// - Input validation works correctly
    /// - Validation errors are properly logged
    /// - Warning level logs are generated for validation failures
    /// - The logging middleware captures validation errors
    [Fact]
   // [Fact(DisplayName = "LOG-003 Invalid payload returns BadRequest")]
    public async Task LOG_003_InvalidPayload_ReturnsBadRequest()
    {
        // Arrange: Create invalid request
        var request = CreateInvalidCustomerRequest();

        // Act: Submit invalid request
        var response = await _client.PostAsJsonAsync("/api/Customer/sync", request);

        // Assert: Verify validation failure
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // TODO: Implement log verification
        // Verify Warning log was written with:
        // - Log level: Warning
        // - Message contains validation error details
        // - Invalid field information is included
        // - Request ID is included
        // - User information is included
    }

    #endregion
}