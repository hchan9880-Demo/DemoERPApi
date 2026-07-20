namespace DemoERPApi.Models;


/// Request model for user login.

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}