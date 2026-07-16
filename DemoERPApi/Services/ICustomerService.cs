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

        Task<bool> DeleteCustomerAsync(
            string crmCustomerId);
    }
}