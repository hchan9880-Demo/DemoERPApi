namespace DemoERPApi.Models;


/// DTO for login response with access and refresh tokens.

public class LoginResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}