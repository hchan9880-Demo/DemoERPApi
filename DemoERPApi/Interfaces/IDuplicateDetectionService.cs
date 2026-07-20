using DemoERPApi.Models;

namespace DemoERPApi.Interfaces;


/// Interface for duplicate customer detection (CRM ID, Email).

public interface IDuplicateDetectionService
{
    
    /// Checks if a CRM Customer ID already exists.
    
    Task<bool> IsDuplicateCustomerIdAsync(string crmCustomerId);

    
    /// Checks if an email is already in use.
    
    Task<bool> IsDuplicateEmailAsync(string email);

    
    /// Checks if either CRM ID or Email is a duplicate.
    
    Task<bool> HasDuplicateAsync(Customers customer);

    
    /// Returns a descriptive duplicate reason message.
    
    Task<string?> GetDuplicateReasonAsync(Customers customer);
}