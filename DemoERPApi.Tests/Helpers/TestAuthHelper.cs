using System.IdentityModel.Tokens.Jwt;
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

    // =====================================================
    // NEGATIVE TEST SUPPORT
    // =====================================================

    public static void SetInvalidToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "INVALID_TOKEN_123");
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
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
    new Claim("username", username),
    new Claim(ClaimTypes.Name, username),

    new Claim(ClaimTypes.Role, role),
    new Claim("role", role),

    // optional compatibility
    new Claim("name", username)
};

        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", jwt);
    }
}