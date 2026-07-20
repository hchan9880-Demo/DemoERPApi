namespace DemoERPApi.Models;


/// Request model for refreshing an access token.

public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}