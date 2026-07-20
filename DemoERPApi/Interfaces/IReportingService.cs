using DemoERPApi.Models;

namespace DemoERPApi.Interfaces;


/// Interface for reporting and analytics operations.

public interface IReportingService
{
    
    /// Returns total, active, and deleted customer counts.
    
    Task<CustomerSummaryReportDto> GetCustomerSummaryAsync();

    
    /// Returns recent sync logs for the specified number of days.
    
    Task<List<SyncReportDto>> GetRecentSyncsAsync(int days);

    
    /// Returns all soft-deleted customers.
    
    Task<List<Customers>> GetDeletedCustomersAsync();

    
    /// Returns duplicate customers by email (active only).
    
    Task<List<DuplicateCustomersDto>> GetDuplicateCustomersAsync();

    
    /// Returns counts of customers with missing required fields.
    
    Task<DataQualityReportDto> GetDataQualityReportAsync();

    
    /// Returns API usage statistics from audit logs.
    
    Task<ApiUsageReportDto> GetApiUsageStatisticsAsync();

    
    /// Returns system health including database connectivity.
    
    Task<SystemHealthReportDto> GetSystemHealthAsync();
}