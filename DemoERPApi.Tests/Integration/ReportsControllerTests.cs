using System.Net;
using System.Net.Http.Json;
using DemoERPApi.Tests.Helpers;
using Xunit;

namespace DemoERPApi.Tests.Integration
{
    public class ReportsControllerTests
        : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;

        public ReportsControllerTests(
            TestApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CustomerSummary_Returns_OK()
        {
            // Arrange
            TestAuthHelper.SetAdminToken(_client);


            // Act
            var response =
                await _client.GetAsync(
                    "/api/reports/customer-summary"
                );


            // Assert
            Assert.Equal(
                HttpStatusCode.OK,
                response.StatusCode
            );
        }


        [Fact]
        public async Task CustomerSummary_WithoutToken_Returns_Unauthorized()
        {
            // Act

            var response =
                await _client.GetAsync(
                    "/api/reports/customer-summary"
                );


            // Assert

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode
            );
        }





        [Fact]
        public async Task CustomerSummary_WithAdminToken_Returns_Data()
        {
            // Arrange

            TestAuthHelper.SetAdminToken(_client);


            // Act

            var response =
                await _client.GetAsync(
                    "/api/reports/customer-summary"
                );


            // Assert

            response.EnsureSuccessStatusCode();


            var content =
                await response.Content.ReadAsStringAsync();


            Assert.NotNull(content);
        }

    }
}