using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DemoERPApi.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;


    public TokenService(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }



    public string GenerateToken(
        string username,
        string role)
    {

        var key =
            _configuration["Jwt:Key"]
            ?? throw new Exception("JWT Key missing");


        var issuer =
            _configuration["Jwt:Issuer"]
            ?? throw new Exception("JWT Issuer missing");


        var audience =
            _configuration["Jwt:Audience"]
            ?? throw new Exception("JWT Audience missing");



        // ======================================
        // TEMP JWT DEBUG
        // ======================================

        Console.WriteLine("==============================");
        Console.WriteLine("JWT TOKEN GENERATION SETTINGS");
        Console.WriteLine($"Issuer: {issuer}");
        Console.WriteLine($"Audience: {audience}");
        Console.WriteLine($"Key Length: {key.Length}");
        Console.WriteLine("==============================");



        var securityKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));



        var credentials =
            new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);



        var claims =
            new List<Claim>
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    username),


                new Claim(
                    ClaimTypes.Name,
                    username),


                new Claim(
                    ClaimTypes.Role,
                    role),


                // Keep your existing role claim
                new Claim(
                    "role",
                    role),


                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };



        var token =
            new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires:
                    DateTime.UtcNow.AddHours(1),
                signingCredentials:
                    credentials);



        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}