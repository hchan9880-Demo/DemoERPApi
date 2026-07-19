using DemoERPApi.Models;

namespace DemoERPApi.Interfaces
{
    public interface IReportingService
    {
        Task<CustomerSummaryReportDto> GetCustomerSummaryAsync();

        Task<List<SyncReportDto>> GetRecentSyncsAsync(int days);

        Task<List<Customers>> GetDeletedCustomersAsync();

        Task<List<DuplicateCustomersDto>> GetDuplicateCustomersAsync();

        Task<DataQualityReportDto> GetDataQualityReportAsync();

        Task<ApiUsageReportDto> GetApiUsageStatisticsAsync();

        Task<SystemHealthReportDto> GetSystemHealthAsync();
    }
}