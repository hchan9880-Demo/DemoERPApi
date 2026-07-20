using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DemoERPApi.Tests.Integration;


/// Integration tests for the HTTP middleware pipeline.
/// Validates that the application correctly handles authentication,
/// authorization, routing, and error scenarios.

public class MiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    // Test data constants
    private const string TestCustomerId = "TEST_CUSTOMER_001";
    private const string NonExistentEndpoint = "/api/NonExistentEndpoint";

    
    /// Initializes a new instance of the <see cref="MiddlewareTests"/> class.
    
    /// <param name="factory">Web application factory for integration testing</param>
    public MiddlewareTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    
    /// Tests that protected endpoints return 401 Unauthorized when no JWT is provided.
    /// 
    /// Workflow:
    /// 1. Create an unauthenticated client (no Authorization header)
    /// 2. Attempt to access a protected endpoint
    /// 3. Verify the authentication middleware rejects the request
    /// 4. Confirm response is 401 Unauthorized
    /// 
    /// This test ensures:
    /// - Authentication middleware is correctly configured
    /// - Protected endpoints require authentication
    /// - Unauthorized requests are properly rejected
    /// - The [Authorize] attribute is being enforced
    
    [Fact]
    public async Task MissingJwt_Returns401()
    {
        // Arrange: Create an unauthenticated client
        var client = _factory.CreateClient();

        // Act: Request a protected endpoint without an Authorization header
        var response = await client.GetAsync($"/api/Customer/{TestCustomerId}");

        // Assert: Expect 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    
    /// Tests that requests to non-existent routes return 404 Not Found.
    /// 
    /// Workflow:
    /// 1. Create a test client
    /// 2. Send a GET request to an invalid endpoint
    /// 3. Verify the routing middleware returns 404
    /// 4. Confirm the application handles invalid routes gracefully
    /// 
    /// This test ensures:
    /// - Routing middleware is correctly configured
    /// - Invalid URLs are properly handled
    /// - No exception is thrown for invalid routes
    /// - The application returns consistent 404 responses
    
    [Fact]
    public async Task UnknownRoute_Returns404()
    {
        // Arrange: Create a test client
        var client = _factory.CreateClient();

        // Act: Request a route that does not exist
        var response = await client.GetAsync(NonExistentEndpoint);

        // Assert: Expect 404 Not Found
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}