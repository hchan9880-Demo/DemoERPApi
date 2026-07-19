using DemoERPApi.Data;
using DemoERPApi.Interfaces;
using DemoERPApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoERPApi.Services
{
    public class ReportingService : IReportingService
    {
        private readonly AppDbContext _context;

        public ReportingService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<CustomerSummaryReportDto> GetCustomerSummaryAsync()
        {
            return new CustomerSummaryReportDto
            {
                TotalCustomers = await _context.Customers.CountAsync(),

                ActiveCustomers = await _context.Customers
                    .CountAsync(c => !c.IsDeleted),

                DeletedCustomers = await _context.Customers
                    .CountAsync(c => c.IsDeleted)
            };
        }


        public async Task<List<SyncReportDto>> GetRecentSyncsAsync(int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days);


            return await _context.SyncLogs
                .Where(x => x.CreatedDate >= startDate)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new SyncReportDto
                {
                    CustomerId = x.CustomerID,
                    CRMCustomerID = x.Customer!.CRMCustomerID,
                    Status = x.Status,
                    SyncDate = x.CreatedDate
                })
                .ToListAsync();
        }


        public async Task<List<Customers>> GetDeletedCustomersAsync()
        {
            return await _context.Customers
                .Where(c => c.IsDeleted)
                .ToListAsync();
        }



        public async Task<List<DuplicateCustomersDto>> GetDuplicateCustomersAsync()
        {
            var duplicates = await _context.Customers
                .Where(c => !c.IsDeleted)
                .GroupBy(c => c.Email)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .Select(c => new DuplicateCustomersDto
                {
                    CustomerId = c.CustomerID,
                    CRMCustomerID = c.CRMCustomerID,
                    Email = c.Email,
                    FullName = c.FirstName + " " + c.LastName
                })
                .ToListAsync();


            return duplicates;
        }



        public async Task<DataQualityReportDto> GetDataQualityReportAsync()
        {
            return new DataQualityReportDto
            {
                MissingEmailCount = await _context.Customers
                    .CountAsync(c => string.IsNullOrEmpty(c.Email)),


                MissingPhoneCount = await _context.Customers
                    .CountAsync(c => string.IsNullOrEmpty(c.Phone)),


                MissingNameCount = await _context.Customers
                    .CountAsync(c =>
                        string.IsNullOrEmpty(c.FirstName) ||
                        string.IsNullOrEmpty(c.LastName))
            };
        }




        public async Task<ApiUsageReportDto> GetApiUsageStatisticsAsync()
        {
            var total = await _context.AuditLogs.CountAsync();

            return new ApiUsageReportDto
            {
                TotalRequests = total,

                // Everything successfully written to AuditLogs
                SuccessfulRequests = total,

                FailedRequests = 0
            };
        }





/*
        public async Task<ApiUsageReportDto> GetApiUsageStatisticsAsync()
        {
            return new ApiUsageReportDto
            {
                TotalRequests = await _context.AuditLogs.CountAsync(),

                SuccessfulRequests = await _context.AuditLogs
                    .CountAsync(x => x.StatusCode >= 200 &&
                                     x.StatusCode < 300),

                FailedRequests = await _context.AuditLogs
                    .CountAsync(x => x.StatusCode >= 400)

            };
        }
*/


        public async Task<SystemHealthReportDto> GetSystemHealthAsync()
        {
            var databaseConnected = await _context.Database
                .CanConnectAsync();


            return new SystemHealthReportDto
            {
                ApiStatus = "Running",

                DatabaseStatus = databaseConnected
                    ? "Healthy"
                    : "Unavailable",

                ServerTime = DateTime.UtcNow
            };
        }
    }
}