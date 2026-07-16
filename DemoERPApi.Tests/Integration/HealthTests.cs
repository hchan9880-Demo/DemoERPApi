using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DemoERPApi.Tests.Integration
{
    // We use IClassFixture to spin up an in-memory instance of your API
    public class HealthTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public HealthTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Health_ReturnsHealthy()
        {
            // Arrange: Create a test client
            var client = _factory.CreateClient();

            // Act: Call the health endpoint we defined in Program.cs
            var response = await client.GetAsync("/health");

            // Assert: Verify status 200 OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}