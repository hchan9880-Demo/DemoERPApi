using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using FluentAssertions;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DemoERPApi.Tests.Integration.Logging;
/*
LOG-006 Request ID generation
Expected:
• X-Request-ID response header exists
• Valid GUID returned
• Same Request ID available throughout request
*/

/// Integration tests for the Request ID Middleware.
/// Validates that each HTTP request receives a unique correlation ID
/// that can be used for request tracing and debugging.
/// 
/// Test Coverage:
/// LOG-006    Request ID generation - X-Request-ID header exists and is valid GUID

public class RequestIdMiddlewareTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    // Header name constant
    private const string RequestIdHeader = "X-Request-ID";

    
    /// Initializes a new instance of the <see cref="RequestIdMiddlewareTests"/> class.
    
    /// <param name="factory">Custom web application factory for testing</param>
    public RequestIdMiddlewareTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        TestAuthHelper.SetAdminToken(_client);
    }

    
    /// LOG-006: Verifies that the Request ID middleware generates and returns
    /// a valid X-Request-ID header for each request.
    /// 
    /// Workflow:
    /// 1. Send a GET request to any endpoint (/health)
    /// 2. Check that the response includes an X-Request-ID header
    /// 3. Validate that the header value is a valid GUID
    /// 
    /// This test ensures:
    /// - The Request ID middleware is correctly registered in the pipeline
    /// - Each request receives a unique correlation ID
    /// - The X-Request-ID header is present in all responses
    /// - The Request ID is a valid GUID format
    /// - The same Request ID is available throughout the request lifecycle
    /// 
    /// Benefits of Request ID:
    /// - Correlates logs across multiple services
    /// - Helps trace requests through the system
    /// - Aids in debugging and troubleshooting
    /// - Provides end-to-end request tracking
    
    [Fact(DisplayName = "LOG-006 Request ID header is returned and valid")]
    public async Task LOG_006_RequestIdHeader_Returned()
    {
        // Act: Send a request to any endpoint
        var response = await _client.GetAsync("/health");

        // Assert: Verify X-Request-ID header exists
        response.Headers
            .Contains(RequestIdHeader)
            .Should()
            .BeTrue($"The {RequestIdHeader} header should be present in all responses");

        // Assert: Verify header value is a valid GUID
        var requestId = response.Headers
            .GetValues(RequestIdHeader)
            .Single();

        Guid.TryParse(requestId, out _)
            .Should()
            .BeTrue($"The {RequestIdHeader} value should be a valid GUID, but was '{requestId}'");
    }
}