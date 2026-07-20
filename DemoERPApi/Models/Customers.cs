using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoERPApi.Models;


/// Represents a customer entity in the system.

[Table("Customers")]
public class Customers
{
    [Key]
    [Column("CustomerID")]
    public int CustomerID { get; set; }

    public string CRMCustomerID { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime LastUpdated { get; set; }

    [NotMapped]
    public string? Role { get; set; }
}