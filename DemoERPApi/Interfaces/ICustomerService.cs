using DemoERPApi.Models;

namespace DemoERPApi.Interfaces;


/// Interface for customer CRUD and sync operations.

public interface ICustomerService
{
    
    /// Syncs a customer from external system with audit and sync logging.
    
    Task<Customers> SyncCustomerAsync(CustomersDto customer, string userName);

    
    /// Retrieves an active customer by CRM ID.
    
    Task<Customers?> GetCustomerAsync(string crmCustomerId);

    
    /// Retrieves all active (non-deleted) customers.
    
    Task<IEnumerable<Customers>> GetCustomersAsync();

    
    /// Soft-deletes a customer by CRM ID.
    
    Task<bool> DeleteCustomerAsync(string crmCustomerId);
}