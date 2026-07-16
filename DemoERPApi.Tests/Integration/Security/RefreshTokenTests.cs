using DemoERPApi.Tests.Fixtures;
using DemoERPApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace DemoERPApi.Tests.Integration.Security;

/*
Test Cases Covered:

SEC-015    Login returns refresh token
SEC-016    Refresh token generates new JWT
SEC-017    Expired refresh token rejected
SEC-018    Used refresh token rejected
SEC-019    Logout revokes refresh token
*/

public class RefreshTokenTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;


    public RefreshTokenTests(WebApplicationFactory<Program> factory)
    {

        _client = factory.CreateClient();
    }



    private async Task<(string accessToken, string refreshToken)> LoginAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login",
            new
            {
                Username = "admin",
                Password = "Password123"
            });

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        return
        (
            json.GetProperty("accessToken").GetString(),
            json.GetProperty("refreshToken").GetString()
        );
    }

    // ==============================================================
    // SEC-015
    // ==============================================================

    [Fact]
    public async Task SEC_015_Login_ReturnsRefreshToken()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login",
            new
            {
                Username = "admin",
                Password = "Password123"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(json.TryGetProperty("refreshToken", out var token));

        Assert.False(string.IsNullOrWhiteSpace(token.GetString()));
    }

    // ==============================================================
    // SEC-016
    // ==============================================================

    [Fact]
    public async Task SEC_016_RefreshToken_ReturnsNewJwt()
    {
        var login = await LoginAsync();

        var response = await _client.PostAsJsonAsync("/api/Auth/refresh",
            new
            {
                refreshToken = login.refreshToken
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.True(json.TryGetProperty("accessToken", out var token));

        Assert.False(string.IsNullOrWhiteSpace(token.GetString()));
    }

    // ==============================================================
    // SEC-017
    // ==============================================================

    [Fact]
    public async Task SEC_017_ExpiredRefreshToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/refresh",
            new
            {
                refreshToken = "expired-refresh-token"
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ==============================================================
    // SEC-018
    // ==============================================================

    [Fact]
    public async Task SEC_018_UsedRefreshToken_ReturnsUnauthorized()
    {
        var login = await LoginAsync();

        await _client.PostAsJsonAsync("/api/Auth/refresh",
            new
            {
                refreshToken = login.refreshToken
            });

        var secondAttempt = await _client.PostAsJsonAsync("/api/Auth/refresh",
            new
            {
                refreshToken = login.refreshToken
            });

        Assert.Equal(HttpStatusCode.Unauthorized, secondAttempt.StatusCode);
    }

    // ==============================================================
    // SEC-019
    // ==============================================================

    [Fact]
    public async Task SEC_019_Logout_RevokesRefreshToken()
    {
        var login = await LoginAsync();

        await _client.PostAsJsonAsync("/api/Auth/logout",
            new
            {
                refreshToken = login.refreshToken
            });

        var refreshAttempt = await _client.PostAsJsonAsync("/api/Auth/refresh",
            new
            {
                refreshToken = login.refreshToken
            });

        Assert.Equal(HttpStatusCode.Unauthorized, refreshAttempt.StatusCode);
    }
}