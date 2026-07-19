using DemoERPApi.Models;

namespace DemoERPApi.Interfaces
{
    public interface ICustomerService
    {
        Task<Customers> SyncCustomerAsync(
            CustomersDto customer,
            string userName);

        Task<Customers?> GetCustomerAsync(
            string crmCustomerId);

        // Fixed: Renamed to Async to match implementation
        Task<IEnumerable<Customers>> GetCustomersAsync();

        Task<bool> DeleteCustomerAsync(
            string crmCustomerId);
    }
}