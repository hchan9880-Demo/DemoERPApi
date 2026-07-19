using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoERPApi.Models
{
    [Table("Customers")]
    public class Customers
    {
        [Key]
        [Column("CustomerID")] // Explicitly mapped to the int column
        public int CustomerID { get; set; }

        public string CRMCustomerID { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime LastUpdated { get; set; }

        // REMOVE any property here that does not appear in your sp_help output.
        // If you absolutely must keep a property, mark it with [NotMapped].
        [NotMapped]
        public string? Role { get; set; }
    }
}