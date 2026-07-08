namespace DemoERPApi.Models;

public class CustomerDto
{
    public string CRMCustomerID { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public bool IsDeleted{ get; set; }
}

