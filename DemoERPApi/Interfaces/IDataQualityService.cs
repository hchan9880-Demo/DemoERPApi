using DemoERPApi.Models;

namespace DemoERPApi.Interfaces;


/// Interface for validating customer data quality.

public interface IDataQualityService
{
    
    /// Validates a customer's data quality (email, phone, etc.).
    
    DataQualityResult ValidateCustomer(CustomersDto customer);
}