using System.ComponentModel.DataAnnotations;

namespace DemoERPApi.Models;


/// Represents a refresh token for JWT authentication.

public class RefreshToken
{
    [Key]
    public int TokenID { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? RevokedDate { get; set; }
    public bool IsUsed { get; set; }
    public string CreatedByIP { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}