using Azure.Core;
using DemoERPApi.Data;
using DemoERPApi.Models;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using DemoERPApi.Interfaces;
namespace DemoERPApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthController(AppDbContext context, IConfiguration configuration, IRefreshTokenService refreshTokenService)
    {
        _context = context;
        _configuration = configuration;
        _refreshTokenService = refreshTokenService;

    }

    // =====================================================
    // REGISTER
    // =====================================================
    [HttpPost("register")]
    public IActionResult Register([FromBody] DemoERPApi.Models.RegisterRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and Password are required.");
        }

        var existingUser = _context.Users
            .FirstOrDefault(u => u.Username == request.Username);

        if (existingUser != null)
        {
            return BadRequest("User already exists");
        }

        var user = new User
        {
            Username = request.Username,
            Role = "User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok("User created");
    }

    // =====================================================
    // GENERATE HASH
    // =====================================================
    [HttpGet("generatehash")]
    public IActionResult GenerateHash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return BadRequest("Password parameter is required.");
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        return Ok(hash);
    }



    // =====================================================
    // REFRESH TOKEN
    // =====================================================
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .AsTracking()
            .SingleOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (storedToken == null)
            return Unauthorized();

        if (storedToken.RevokedDate.HasValue)
            return Unauthorized();

        if (storedToken.IsUsed)
            return Unauthorized();

        if (storedToken.ExpirationDate <= DateTime.UtcNow)
            return Unauthorized();

        storedToken.IsUsed = true;

        var user = await _context.Users
            .SingleOrDefaultAsync(x => x.UserId == storedToken.UserId);

        if (user == null)
            return Unauthorized();

        var newRefreshToken = _refreshTokenService.GenerateRefreshToken(
            user.UserId,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");

        _context.RefreshTokens.Add(newRefreshToken);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken = GenerateToken(user.Username, user.Role),
            refreshToken = newRefreshToken.Token
        });
    }



    // =====================================================
    // LOGIN
    // =====================================================
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and Password are required.");
        }

        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
            return Unauthorized();

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized();

        var refreshToken = _refreshTokenService.GenerateRefreshToken(
            user.UserId,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");

        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync();

        var accessToken = GenerateToken(user.Username, user.Role);

        return Ok(new
        {
            accessToken,
            refreshToken = refreshToken.Token
        });
    }


    // =====================================================
    // LOGOUT
    // =====================================================
 
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var token = await _context.RefreshTokens
            .AsTracking()
            .SingleOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (token == null)
            return Unauthorized();

        if (token.RevokedDate != null)
            return Unauthorized();

        token.RevokedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok();
    }

















    // =====================================================
    // JWT TOKEN GENERATION
    // =====================================================
    private string GenerateToken(string username, string role)
    {
        // Fall back to the exact hardcoded keys matching Program.cs if missing from configuration
        var keyStr = _configuration["JwtSettings:Key"] ?? "ThisIsATestJwtKey123456789012345";
        var issuer = _configuration["JwtSettings:Issuer"] ?? "DemoERPApi";
        var audience = _configuration["JwtSettings:Audience"] ?? "DemoERPApiUsers";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("username", username),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim("role", role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    [Authorize]
    [HttpGet("whoami")]
    public IActionResult WhoAmI()
    {
        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated,

            Username =
                User.Identity?.Name,

            Claims =
                User.Claims.Select(c => new
                {
                    c.Type,
                    c.Value
                })
        });
    }









}