using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace DemoERPApi.Tests.Helpers;

public static class TestAuthHelper
{
    private const string JwtKey =
        "DemoERP_Test_JWT_Key_12345678901234567890";


    private const string JwtIssuer =
        "DemoERPApi";


    private const string JwtAudience =
        "DemoERPApiUsers";



    // =====================================================
    // REAL LOGIN TOKEN
    // =====================================================
    public static void SetAdminToken(HttpClient client)
    {
        var response =
            client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    Username = "admin",
                    Password = "Password123"
                })
            .Result;


        var json =
            response.Content
                .ReadAsStringAsync()
                .Result;


        var result =
            JsonSerializer.Deserialize<LoginResultDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


        if (result == null || string.IsNullOrEmpty(result.Token))
        {
            throw new Exception(
                $"Admin login failed. Response: {json}");
        }


        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                result.Token);
    }





    // =====================================================
    // CUSTOMER ROLE TOKEN
    // =====================================================
    public static void SetOwnerToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                CreateJwtToken(
                    "owner1",
                    "Customer",
                    "CRM103"));
    }





    // =====================================================
    // QA ROLE TOKEN
    // =====================================================
    public static void SetQAToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                CreateJwtToken(
                    "qauserB",
                    "QA",
                    "CRM106"));
    }





    // =====================================================
    // GENERIC ROLE TOKEN
    // =====================================================
    public static void SetTokenWithRole(
        HttpClient client,
        string? role)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                CreateJwtToken(
                    "test-user",
                    role,
                    "CRM001"));
    }




    // =====================================================
    // TOKEN WITHOUT ROLE CLAIM
    // Used by AUTH missing-role tests
    // =====================================================
    public static void SetTokenWithoutRole(
        HttpClient client)
    {
        var claims = new[]
        {
            new Claim(
                "username",
                "test-user"),


            new Claim(
                ClaimTypes.Name,
                "test-user"),


            new Claim(
                JwtRegisteredClaimNames.Sub,
                "test-user")
        };


        var token =
            CreateToken(claims);


        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);
    }





    // =====================================================
    // CLEAR TOKEN
    // =====================================================
    public static void ClearToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }





    // =====================================================
    // INVALID JWT
    // =====================================================
    public static void SetInvalidToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                "INVALID_TOKEN");
    }





    // =====================================================
    // EXPIRED JWT
    // =====================================================
    public static void SetExpiredToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                CreateExpiredJwtToken());
    }





    // =====================================================
    // TAMPERED JWT
    // =====================================================
    public static void SetTamperedToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                "eyJhbGciOiJIUzI1NiJ9.TAMPERED.SIGNATURE");
    }





    // =====================================================
    // UNAUTHORIZED ROLE
    // =====================================================
    public static void SetUnauthorizedUserToken(
        HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                CreateJwtToken(
                    "unauthorized-user",
                    "Unauthorized",
                    "CRM999"));
    }





    // =====================================================
    // CREATE VALID JWT
    // Match AuthController production claims
    // =====================================================
    private static string CreateJwtToken(
        string username,
        string? role,
        string? customerId)
    {
        var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, username),
    new Claim("CustomerId", customerId)
};

        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }


        return CreateToken(claims);
    }





    // =====================================================
    // CREATE TOKEN COMMON METHOD
    // =====================================================
    private static string CreateToken(
       IEnumerable<Claim> claims)
    {
        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(JwtKey));


        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);


        var token =
            new JwtSecurityToken(
                issuer: JwtIssuer,
                audience: JwtAudience,
                claims: claims,
                expires:
                    DateTime.UtcNow.AddMinutes(30),
                signingCredentials:
                    credentials);


        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }



    // =====================================================
    // CREATE EXPIRED JWT
    // =====================================================
    private static string CreateExpiredJwtToken()
    {
        var claims = new[]
        {
            new Claim(
                "username",
                "expired-user"),


            new Claim(
                ClaimTypes.Name,
                "expired-user"),


            new Claim(
                ClaimTypes.Role,
                "Customer"),


            new Claim(
                "role",
                "Customer")
        };


        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(JwtKey));


        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);



        var token =
            new JwtSecurityToken(
                issuer: JwtIssuer,
                audience: JwtAudience,
                claims: claims,
                expires:
                    DateTime.UtcNow.AddMinutes(-10),
                signingCredentials:
                    credentials);



        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}