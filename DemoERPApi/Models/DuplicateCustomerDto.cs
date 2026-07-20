namespace DemoERPApi.Models;


/// DTO for duplicate customer details.

public class DuplicateCustomersDto
{
    public int CustomerId { get; set; }
    public string CRMCustomerID { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}