using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public static class TestAuthHelper
{
    private const string JwtKey = "ThisIsATestJwtKey123456789012345";
    private const string JwtIssuer = "DemoERPApi";
    private const string JwtAudience = "DemoERPApiUsers";

    // =====================================================
    // ORIGINAL / REQUIRED TEST METHODS (DO NOT REMOVE)
    // =====================================================

    public static void SetAdminToken(HttpClient client)
        => SetToken(client, "admin", "Admin");

    public static void SetOwnerToken(HttpClient client)
        => SetToken(client, "owner1", "Customer");

    public static void SetUnauthorizedUserToken(HttpClient client)
        => SetToken(client, "user1", "Unauthorized");

    // =====================================================
    // ROLE VARIANTS (USED BY NEW TESTS / CLEAN STRUCTURE)
    // =====================================================

    public static void SetCustomerToken(HttpClient client)
        => SetToken(client, "customer1", "Customer");

    public static void SetQAToken(HttpClient client)
        => SetToken(client, "qa1", "QA");

    public static void SetTokenWithRole(HttpClient client, string role)
        => SetToken(client, "test_user", role);

    // =====================================================
    // NEGATIVE TEST SUPPORT
    // =====================================================

    public static void SetInvalidToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "INVALID_TOKEN_123");
    }

    /// <summary>
    /// Generates a signature with an expiration timestamp explicitly configured in the past.
    /// </summary>
    public static void SetExpiredToken(HttpClient client)
    {
        var jwt = GenerateRawToken("expired_user", "Admin", DateTime.UtcNow.AddHours(-2));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
    }

    /// <summary>
    /// Generates a valid signature token and deliberately alters characters in the signature segment.
    /// </summary>
    public static void SetTamperedToken(HttpClient client)
    {
        var validJwt = GenerateRawToken("tampered_user", "Admin", DateTime.UtcNow.AddHours(1));
        // Append characters to invalidate the cryptographic verification check block
        var tamperedJwt = validJwt + "xyz";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tamperedJwt);
    }

    public static void ClearToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }

    // =====================================================
    // CORE JWT CREATION (DO NOT MODIFY LOGIC)
    // =====================================================

    private static void SetToken(HttpClient client, string username, string role)
    {
        var jwt = GenerateRawToken(username, role, DateTime.UtcNow.AddHours(1));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", jwt);
    }

    /// <summary>
    /// Extracted helper to isolate string signing tasks while preserving the core generation logic rules.
    /// </summary>
    private static string GenerateRawToken(string username, string role, DateTime expiration)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var safeRole = role ?? string.Empty;
        var safeUsername = username ?? "anonymous";

        var claims = new[]
        {
            new Claim("username", safeUsername),
            new Claim(ClaimTypes.Name, safeUsername),
            new Claim(ClaimTypes.Role, safeRole),
            new Claim("role", safeRole),
            new Claim("name", safeUsername)
        };

        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}