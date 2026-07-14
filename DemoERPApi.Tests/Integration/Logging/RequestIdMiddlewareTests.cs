using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using Xunit;
using FluentAssertions;
namespace DemoERPApi.Tests.Integration.Logging;

/*
LOG-006 Request ID generation
Expected:
• X-Request-ID response header exists
• Valid GUID returned
• Same Request ID available throughout request
*/



    public class RequestIdMiddlewareTests
    : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient Client;

        public RequestIdMiddlewareTests(CustomWebApplicationFactory factory)
        {
            Client = factory.CreateClient();

            TestAuthHelper.SetAdminToken(Client);
        }




        [Fact(DisplayName="LOG-006 RequestId generated")]
    public async Task LOG_006_RequestIdHeader_Returned()
    {
        var response =
            await Client.GetAsync("/health");

        response.Headers
            .Contains("X-Request-ID")
            .Should()
            .BeTrue();

        var requestId =
            response.Headers
                .GetValues("X-Request-ID")
                .Single();

        Guid.TryParse(requestId, out _)
            .Should()
            .BeTrue();
    }
}
