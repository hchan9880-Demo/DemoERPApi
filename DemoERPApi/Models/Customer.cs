using System.ComponentModel.DataAnnotations;

namespace DemoERPApi.Models
{
    public class Customer
    {
        [Key]
        public string CRMCustomerID { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        public DateTime LastUpdated { get; set; }

    }
}
