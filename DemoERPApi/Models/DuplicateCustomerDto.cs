namespace DemoERPApi.Models
{
    public class DuplicateCustomersDto
    {
        public int CustomerId { get; set; }

        public string CRMCustomerID { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }
    }
}