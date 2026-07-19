using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public int UserId { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }       // Matches Role column
    public string CustomerID { get; set; } // Matches string CustomerID column
    public bool IsActive { get; set; }
}