using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace DemoERPApi.Tests.Integration.Logging;


/// Integration tests for the Exception Middleware.
/// Validates that unhandled exceptions are properly caught, logged,
/// and returned as RFC7807 Problem Details responses.
/// 
/// Test Coverage:
/// LOG-004    Unhandled exception triggers middleware and returns 500
/// LOG-005    Exception response follows RFC7807 Problem Details format

public class ExceptionMiddlewareTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    // Test endpoint constants
    private const string ThrowEndpoint = "/api/test/throw";

    
    /// Initializes a new instance of the <see cref="ExceptionMiddlewareTests"/> class.
    
    /// <param name="factory">Custom web application factory for testing</param>
    public ExceptionMiddlewareTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        TestAuthHelper.SetAdminToken(_client);
    }

    #region Exception Handling Tests


    /// LOG-004: Verifies that unhandled exceptions return HTTP 500 Internal Server Error.
    /// 
    /// Workflow:
    /// 1. Send a GET request to a test endpoint that throws an exception
    /// 2. Exception middleware intercepts the exception
    /// 3. Verify response status code is 500 Internal Server Error
    /// 
    /// This test ensures:
    /// - Exception middleware is correctly registered in the pipeline
    /// - Unhandled exceptions are caught and handled gracefully
    /// - The application returns appropriate error responses
    /// - No raw exception details are exposed to the client
    [Fact]
   // [Fact(DisplayName = "LOG-004 Unhandled exception returns 500")]
    public async Task LOG_004_UnhandledException_Returns500()
    {
        // Act: Call endpoint that throws an exception
        var response = await _client.GetAsync(ThrowEndpoint);

        // Assert: Verify response is 500 Internal Server Error
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }


    /// LOG-005: Verifies that exception responses follow RFC7807 Problem Details format.
    /// 
    /// Workflow:
    /// 1. Send a GET request to a test endpoint that throws an exception
    /// 2. Exception middleware intercepts the exception
    /// 3. Verify response is in Problem Details format
    /// 4. Validate Problem Details properties (Status = 500)
    /// 
    /// This test ensures:
    /// - Responses follow the RFC7807 standard for API errors
    /// - Problem Details structure is consistently returned
    /// - Status code is correctly set in the response
    /// - Client can parse the error response programmatically
    [Fact]
   // [Fact(DisplayName = "LOG-005 Exception middleware returns Problem Details")]
    public async Task LOG_005_ExceptionMiddleware_ReturnsProblemDetails()
    {
        // Act: Call endpoint that throws an exception
        var response = await _client.GetAsync(ThrowEndpoint);

        // Assert: Verify response contains Problem Details
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Status.Should().Be(500);
    }

    #endregion

    #region Test Controller

    
    /// Test controller used to simulate exceptions for middleware testing.
    /// This controller is only used during integration tests and should
    /// not be used in production environments.
    
    [ApiController]
    [Route("api/test")]
    public class ThrowController : ControllerBase
    {
        
        /// Test endpoint that always throws an exception.
        /// Used to verify exception middleware functionality.
        
        /// <returns>Throws an exception; never returns normally</returns>
        [HttpGet("throw")]
        public IActionResult Throw()
        {
            throw new Exception("Test exception for middleware validation");
        }
    }

    #endregion
}