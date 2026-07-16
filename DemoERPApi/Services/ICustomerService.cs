using DemoERPApi.Models;

namespace DemoERPApi.Services
{
    public interface ICustomerService
    {
        Task<Customer> SyncCustomerAsync(
            CustomerDto customer,
            string userName);

        Task<Customer?> GetCustomerAsync(
            string crmCustomerId);

        // Fixed: Renamed to Async to match implementation
        Task<IEnumerable<Customer>> GetCustomersAsync();

        Task<bool> DeleteCustomerAsync(
            string crmCustomerId);
    }
}