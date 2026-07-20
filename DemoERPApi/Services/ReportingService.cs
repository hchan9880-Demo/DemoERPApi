using DemoERPApi.Data;
using DemoERPApi.Interfaces;
using DemoERPApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoERPApi.Services;


/// Reporting service for customer analytics, data quality, and system health.

public class ReportingService : IReportingService
{
    private readonly AppDbContext _context;

    public ReportingService(AppDbContext context)
    {
        _context = context;
    }

    #region Customer Reports

    
    /// Returns total, active, and deleted customer counts.
    
    public async Task<CustomerSummaryReportDto> GetCustomerSummaryAsync()
    {
        return new CustomerSummaryReportDto
        {
            TotalCustomers = await _context.Customers.CountAsync(),
            ActiveCustomers = await _context.Customers.CountAsync(c => !c.IsDeleted),
            DeletedCustomers = await _context.Customers.CountAsync(c => c.IsDeleted)
        };
    }

    
    /// Returns all soft-deleted customers.
    
    public async Task<List<Customers>> GetDeletedCustomersAsync()
    {
        return await _context.Customers
            .Where(c => c.IsDeleted)
            .ToListAsync();
    }

    
    /// Finds duplicate customers by email (active customers only).
    
    public async Task<List<DuplicateCustomersDto>> GetDuplicateCustomersAsync()
    {
        return await _context.Customers
            .Where(c => !c.IsDeleted)
            .GroupBy(c => c.Email)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g)
            .Select(c => new DuplicateCustomersDto
            {
                CustomerId = c.CustomerID,
                CRMCustomerID = c.CRMCustomerID,
                Email = c.Email,
                FullName = $"{c.FirstName} {c.LastName}".Trim()
            })
            .ToListAsync();
    }

    #endregion

    #region Sync Reports

    
    /// Returns recent sync logs for the specified number of days.
    
    public async Task<List<SyncReportDto>> GetRecentSyncsAsync(int days)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);

        return await _context.SyncLogs
            .Where(x => x.CreatedDate >= startDate)
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new SyncReportDto
            {
                CustomerId = x.CustomerID,
                CRMCustomerID = x.Customer != null ? x.Customer.CRMCustomerID : "Unknown",
                Status = x.Status,
                SyncDate = x.CreatedDate
            })
            .ToListAsync();
    }

    #endregion

    #region Data Quality Reports

    
    /// Returns counts of customers with missing required fields.
    
    public async Task<DataQualityReportDto> GetDataQualityReportAsync()
    {
        return new DataQualityReportDto
        {
            MissingEmailCount = await _context.Customers
                .CountAsync(c => string.IsNullOrEmpty(c.Email)),

            MissingPhoneCount = await _context.Customers
                .CountAsync(c => string.IsNullOrEmpty(c.Phone)),

            MissingNameCount = await _context.Customers
                .CountAsync(c => string.IsNullOrEmpty(c.FirstName) || string.IsNullOrEmpty(c.LastName))
        };
    }

    #endregion

    #region API Usage Reports

    
    /// Returns API usage statistics from audit logs.
    
    public async Task<ApiUsageReportDto> GetApiUsageStatisticsAsync()
    {
        var total = await _context.AuditLogs.CountAsync();

        return new ApiUsageReportDto
        {
            TotalRequests = total,
            SuccessfulRequests = total,
            FailedRequests = 0
        };
    }

    #endregion

    #region System Health Reports

    
    /// Returns current system health including database connectivity.
    
    public async Task<SystemHealthReportDto> GetSystemHealthAsync()
    {
        var databaseConnected = await _context.Database.CanConnectAsync();

        return new SystemHealthReportDto
        {
            ApiStatus = "Running",
            DatabaseStatus = databaseConnected ? "Healthy" : "Unavailable",
            ServerTime = DateTime.UtcNow
        };
    }

    #endregion
}