using DemoERPApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace DemoERPApi.Tests.Integration.Security;


/// Integration tests for refresh token functionality.
/// Validates the complete refresh token lifecycle including generation,
/// usage, expiration, and revocation.
/// 
/// Test Cases Covered:
/// SEC-015    Login returns refresh token
/// SEC-016    Refresh token generates new JWT
/// SEC-017    Expired refresh token rejected
/// SEC-018    Used refresh token rejected
/// SEC-019    Logout revokes refresh token

public class RefreshTokenTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    // Test credentials
    private const string TestUsername = "admin";
    private const string TestPassword = "Password123";
    private const string ExpiredRefreshToken = "expired-refresh-token";

    
    /// Initializes a new instance of the <see cref="RefreshTokenTests"/> class.
    
    /// <param name="factory">Web application factory for integration testing</param>
    public RefreshTokenTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    #region Helper Methods

    
    /// Performs a login request and returns the access and refresh tokens.
    
    /// <returns>Tuple containing access token and refresh token</returns>
    private async Task<(string accessToken, string refreshToken)> LoginAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            Username = TestUsername,
            Password = TestPassword
        });

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        return (
            json.GetProperty("accessToken").GetString()!,
            json.GetProperty("refreshToken").GetString()!
        );
    }

    
    /// Attempts to refresh an access token using a refresh token.
    
    /// <param name="refreshToken">The refresh token to use</param>
    /// <returns>HTTP response from the refresh endpoint</returns>
    private async Task<HttpResponseMessage> RefreshTokenAsync(string refreshToken)
    {
        return await _client.PostAsJsonAsync("/api/Auth/refresh", new
        {
            refreshToken
        });
    }

    
    /// Attempts to logout using a refresh token.
    
    /// <param name="refreshToken">The refresh token to revoke</param>
    /// <returns>HTTP response from the logout endpoint</returns>
    private async Task<HttpResponseMessage> LogoutAsync(string refreshToken)
    {
        return await _client.PostAsJsonAsync("/api/Auth/logout", new
        {
            refreshToken
        });
    }

    
    /// Parses a JSON response and extracts a specific property value.
    
    /// <param name="response">HTTP response containing JSON</param>
    /// <param name="propertyName">Name of the property to extract</param>
    /// <returns>The property value as a string</returns>
    private async Task<string> ExtractJsonPropertyAsync(HttpResponseMessage response, string propertyName)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty(propertyName).GetString()!;
    }

    #endregion

    #region Refresh Token Tests

    
    /// SEC-015: Validates that login successfully returns a refresh token.
    /// 
    /// Workflow:
    /// 1. User submits valid credentials to login endpoint
    /// 2. Server authenticates user
    /// 3. Server generates and returns refresh token
    /// 4. Client receives non-empty refresh token
    
    [Fact]
    public async Task SEC_015_Login_ReturnsRefreshToken()
    {
        // Act: Perform login
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            Username = TestUsername,
            Password = TestPassword
        });

        // Assert: Response is successful
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert: Response contains non-empty refresh token
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("refreshToken", out var token));
        Assert.False(string.IsNullOrWhiteSpace(token.GetString()));
    }

    
    /// SEC-016: Validates that a valid refresh token can generate a new JWT.
    /// 
    /// Workflow:
    /// 1. User logs in and receives refresh token
    /// 2. User submits refresh token to refresh endpoint
    /// 3. Server validates refresh token
    /// 4. Server generates and returns new access token
    /// 5. Client receives new non-empty access token
    
    [Fact]
    public async Task SEC_016_RefreshToken_ReturnsNewJwt()
    {
        // Arrange: Get valid refresh token from login
        var login = await LoginAsync();

        // Act: Use refresh token to get new access token
        var response = await RefreshTokenAsync(login.refreshToken);

        // Assert: Response is successful
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert: Response contains non-empty access token
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("accessToken", out var token));
        Assert.False(string.IsNullOrWhiteSpace(token.GetString()));
    }

    
    /// SEC-017: Validates that expired refresh tokens are rejected.
    /// 
    /// Workflow:
    /// 1. User submits expired refresh token
    /// 2. Server validates token expiration
    /// 3. Token is found to be expired
    /// 4. Server rejects request with 401 Unauthorized
    
    [Fact]
    public async Task SEC_017_ExpiredRefreshToken_ReturnsUnauthorized()
    {
        // Act: Attempt refresh with expired token
        var response = await RefreshTokenAsync(ExpiredRefreshToken);

        // Assert: Should return 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    
    /// SEC-018: Validates that used refresh tokens cannot be reused.
    /// 
    /// Workflow:
    /// 1. User logs in and receives refresh token
    /// 2. User uses refresh token to get new access token (first use)
    /// 3. Server marks refresh token as used
    /// 4. User attempts to reuse the same refresh token (second use)
    /// 5. Server detects token is already used
    /// 6. Server rejects request with 401 Unauthorized
    
    [Fact]
    public async Task SEC_018_UsedRefreshToken_ReturnsUnauthorized()
    {
        // Arrange: Get valid refresh token
        var login = await LoginAsync();

        // Act: First use - should succeed
        var firstResponse = await RefreshTokenAsync(login.refreshToken);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Act: Second use - should fail
        var secondResponse = await RefreshTokenAsync(login.refreshToken);

        // Assert: Should return 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, secondResponse.StatusCode);
    }

    
    /// SEC-019: Validates that logout properly revokes refresh tokens.
    /// 
    /// Workflow:
    /// 1. User logs in and receives refresh token
    /// 2. User logs out using the refresh token
    /// 3. Server revokes (soft-deletes) the refresh token
    /// 4. User attempts to use revoked token for refresh
    /// 5. Server detects token is revoked
    /// 6. Server rejects request with 401 Unauthorized
    
    [Fact]
    public async Task SEC_019_Logout_RevokesRefreshToken()
    {
        // Arrange: Get valid refresh token
        var login = await LoginAsync();

        // Act: Logout to revoke the token
        var logoutResponse = await LogoutAsync(login.refreshToken);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        // Act: Attempt to use revoked token
        var refreshAttempt = await RefreshTokenAsync(login.refreshToken);

        // Assert: Should return 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, refreshAttempt.StatusCode);
    }

    #endregion
}