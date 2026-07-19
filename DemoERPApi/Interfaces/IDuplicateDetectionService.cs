using DemoERPApi.Models;

namespace DemoERPApi.Interfaces
{
    public interface IDuplicateDetectionService
    {
        Task<bool> IsDuplicateCustomerIdAsync(string crmCustomerId);

        Task<bool> IsDuplicateEmailAsync(string email);

        Task<bool> HasDuplicateAsync(Customers customer);

        Task<string?> GetDuplicateReasonAsync(Customers customer);
    }
}