using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers; // Required if you add Auth header tests
using Xunit;

namespace DemoERPApi.Tests
{
    /// <summary>
    /// MiddlewareTests validates the HTTP pipeline (Authentication, Authorization, and Error Handling)
    /// using WebApplicationFactory to spin up an in-memory instance of the API.
    /// </summary>
    public class MiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public MiddlewareTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task MissingJwt_Returns401()
        {
            // Arrange: Create an unauthenticated client
            var client = _factory.CreateClient();

            // Act: Request a protected endpoint without an Authorization header
            var response = await client.GetAsync("/api/Customer/SomeId");

            // Assert: Expect 401 Unauthorized
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UnknownRoute_Returns404()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act: Request a route that does not exist
            var response = await client.GetAsync("/api/NonExistentEndpoint");

            // Assert: Expect 404 Not Found
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}