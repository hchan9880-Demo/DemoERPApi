using DemoERPApi.Data;
using DemoERPApi.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace DemoERPApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // =========================
    // REGISTER
    // =========================
    [HttpPost("register")]
    public IActionResult Register(RegisterRequest request)
    {
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok("User created");
    }


    // =========================
    // generatehash
    // =========================
    [HttpGet("generatehash")]
    public IActionResult GenerateHash(string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        return Ok(hash);
    }

    // =========================
    // LOGIN
    // =========================
    [HttpPost("login")]
    public IActionResult Login(DemoERPApi.Models.LoginRequest request)
    {
        var user = _context.Users
            .FirstOrDefault(u => u.Username == request.Username);

        if (user == null)
        {
            return Unauthorized();
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var token = GenerateToken(user.Username, user.Role);

        return Ok(new { token });
    }

    // =========================
    // JWT TOKEN
    // =========================
    private string GenerateToken(string username, string role)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]));

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
    new Claim("username", username),
    new Claim(ClaimTypes.Name, username),
    new Claim(ClaimTypes.Role, role),
    new Claim("role", role)
};

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}