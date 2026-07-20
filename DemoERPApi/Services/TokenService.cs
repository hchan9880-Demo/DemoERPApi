using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DemoERPApi.Services;


/// JWT token generation service with claims, signing, and expiration.

public class TokenService
{
    private readonly IConfiguration _configuration;

    private const string JwtKeyConfig = "Jwt:Key";
    private const string JwtIssuerConfig = "Jwt:Issuer";
    private const string JwtAudienceConfig = "Jwt:Audience";
    private const int TokenExpiryHours = 1;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    
    /// Generates a JWT access token with user claims.
    
    public string GenerateToken(string username, string role)
    {
        var key = _configuration[JwtKeyConfig]
            ?? throw new Exception($"JWT Key missing. Configure '{JwtKeyConfig}' in app settings.");

        var issuer = _configuration[JwtIssuerConfig]
            ?? throw new Exception($"JWT Issuer missing. Configure '{JwtIssuerConfig}' in app settings.");

        var audience = _configuration[JwtAudienceConfig]
            ?? throw new Exception($"JWT Audience missing. Configure '{JwtAudienceConfig}' in app settings.");

        LogTokenGeneration(issuer, audience, key);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = BuildClaims(username, role);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(TokenExpiryHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    #region Helpers

    private static List<Claim> BuildClaims(string username, string role)
    {
        return new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim("role", role)
        };
    }

    private static void LogTokenGeneration(string issuer, string audience, string key)
    {
        Console.WriteLine("==============================");
        Console.WriteLine("JWT TOKEN GENERATION");
        Console.WriteLine($"Issuer: {issuer}");
        Console.WriteLine($"Audience: {audience}");
        Console.WriteLine($"Key Length: {key.Length} chars");
        Console.WriteLine($"Key Prefix: {key[..Math.Min(10, key.Length)]}...");
        Console.WriteLine($"Expires: {TokenExpiryHours} hour(s)");
        Console.WriteLine("==============================");
    }

    #endregion
}