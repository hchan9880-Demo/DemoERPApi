using System.ComponentModel.DataAnnotations;

namespace DemoERPApi.Models;


/// Represents a system user for authentication and authorization.

public class User
{
    [Key]
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string CustomerID { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}