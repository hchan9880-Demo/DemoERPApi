using DemoERPApi.Models;
using DemoERPApi.Tests.Helpers;
using DemoERPApi.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DemoERPApi.Tests.Integration;

public class AuthControllerTests
: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {

        DatabaseResetHelper.Reset(factory.Services);
        _client = factory.CreateClient();

    }




    // =====================================================
    // LOGIN TESTS
    // =====================================================

    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsAreValid()
    {
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "Password123"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            request
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsInvalid()
    {
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "WrongPassword"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            request
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUsernameIsInvalid()
    {
        var request = new LoginRequest
        {
            Username = "notreal",
            Password = "Password123"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            request
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenUsernameIsMissing()
    {
        var request = new LoginRequest
        {
            Username = null,
            Password = "Password123"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            request
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenPasswordIsMissing()
    {
        var request = new LoginRequest
        {
            Username = "admin",
            Password = null
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            request
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


}
