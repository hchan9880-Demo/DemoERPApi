using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DemoERPApi.Tests.Helpers;

public static class JwtTestTokenHelper
{
    public static string GenerateToken(
        string username = "admin",
        string role = "Admin")
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("DemoERPSecretKey123456789"));

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: "DemoERPApi",
            audience: "DemoERPApiUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}