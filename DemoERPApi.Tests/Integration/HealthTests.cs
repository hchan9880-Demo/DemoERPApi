using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DemoERPApi.Tests.Integration;


/// Health check endpoint tests.
/// Validates that the application's health monitoring endpoint is accessible
/// and returns the expected status.

public class HealthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    
    /// Initializes a new instance of the <see cref="HealthTests"/> class.
    
    /// <param name="factory">Web application factory for integration testing</param>
    public HealthTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    
    /// Tests that the health endpoint returns a 200 OK status.
    /// 
    /// Workflow:
    /// 1. Create a test client from the web application factory
    /// 2. Send a GET request to the /health endpoint
    /// 3. Verify the response status code is 200 OK
    /// 
    /// This test ensures:
    /// - The application is running and responsive
    /// - The health endpoint is properly configured
    /// - Basic routing is working correctly
    /// - The service is ready to accept requests
    
    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        // Arrange: Create a test client for the API
        var client = _factory.CreateClient();

        // Act: Call the health endpoint
        var response = await client.GetAsync("/health");

        // Assert: Verify the response indicates a healthy state
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}