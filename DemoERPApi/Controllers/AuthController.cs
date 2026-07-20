using DemoERPApi.Data;
using DemoERPApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DemoERPApi.Interfaces;

namespace DemoERPApi.Controllers;

/// <summary>
/// Authentication controller for user registration, login, token refresh, and logout.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthController(
        AppDbContext context,
        IConfiguration configuration,
        IRefreshTokenService refreshTokenService)
    {
        _context = context;
        _configuration = configuration;
        _refreshTokenService = refreshTokenService;
    }

    #region Authentication Endpoints

    /// <summary>
    /// Registers a new user.
    /// </summary>
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and Password are required.");
        }

        var existingUser = _context.Users
            .FirstOrDefault(u => u.Username == request.Username);

        if (existingUser != null)
            return BadRequest("User already exists");

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

    /// <summary>
    /// Authenticates user and returns JWT access token with refresh token.
    /// </summary>
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

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .AsTracking()
            .SingleOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (storedToken == null)
            return Unauthorized();

        if (storedToken.RevokedDate.HasValue ||
            storedToken.IsUsed ||
            storedToken.ExpirationDate <= DateTime.UtcNow)
        {
            return Unauthorized();
        }

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

        var accessToken = GenerateToken(user.Username, user.Role);

        return Ok(new
        {
            accessToken,
            refreshToken = newRefreshToken.Token
        });
    }

    /// <summary>
    /// Logs out a user by revoking their refresh token.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var token = await _context.RefreshTokens
            .AsTracking()
            .SingleOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (token == null || token.RevokedDate != null)
            return Unauthorized();

        token.RevokedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Generates a BCrypt hash for testing.
    /// </summary>
    [HttpGet("generatehash")]
    public IActionResult GenerateHash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return BadRequest("Password parameter is required.");

        return Ok(BCrypt.Net.BCrypt.HashPassword(password));
    }

    /// <summary>
    /// Returns current user information from JWT token.
    /// </summary>
    [Authorize]
    [HttpGet("whoami")]
    public IActionResult WhoAmI()
    {
        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated,
            Username = User.Identity?.Name,
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Generates a JWT access token.
    /// </summary>
    private string GenerateToken(string username, string role)
    {
        var keyStr = _configuration["JwtSettings:Key"]
            ?? "ThisIsATestJwtKey123456789012345";
        var issuer = _configuration["JwtSettings:Issuer"]
            ?? "DemoERPApi";
        var audience = _configuration["JwtSettings:Audience"]
            ?? "DemoERPApiUsers";

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

    #endregion
}