using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Xunit;
using FluentAssertions;
using System.Net.Http.Json;

namespace DemoERPApi.Tests.Integration.Logging;

/*
LOG-004 Unhandled exception
Expected:
• Error log written
• ExceptionMiddleware executed

LOG-005 Exception response
Expected:
• HTTP 500
• RFC7807 ProblemDetails returned
*/
public class ExceptionMiddlewareTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient Client;

    public ExceptionMiddlewareTests(CustomWebApplicationFactory factory)
    {
        Client = factory.CreateClient();

        TestAuthHelper.SetAdminToken(Client);
    }










    [Fact(DisplayName="LOG-004 Unhandled exception")]
    public async Task LOG_004_UnhandledException_Returns500()
    {
        var response =
            await Client.GetAsync(
                "/api/test/throw");

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName="LOG-005 RFC7807 response")]
    public async Task LOG_005_ExceptionMiddleware_ReturnsProblemDetails()
    {
        var response =
            await Client.GetAsync(
                "/api/test/throw");

        var problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Status.Should().Be(500);
    }

    [ApiController]
    [Route("api/test")]
    public class ThrowController : ControllerBase
    {
        [HttpGet("throw")]
        public IActionResult Throw()
        {
            throw new Exception("Boom");
        }
    }

}
