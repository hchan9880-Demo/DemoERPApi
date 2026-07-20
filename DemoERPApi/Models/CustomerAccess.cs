using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoERPApi.Models;


/// Represents user access permissions to specific customers.

[Table("CustomerAccess")]
public class CustomerAccess
{
    [Key]
    public int Id { get; set; }

    
    /// External CRM customer identifier (matches Customers.CRMCustomerID).
    
    public string CRMCustomerID { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;
    public int UserId { get; set; }
}