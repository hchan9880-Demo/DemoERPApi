namespace DemoERPApi.Models;


/// Request model for user logout.

public class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}