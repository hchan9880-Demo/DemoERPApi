using DemoERPApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoERPApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportingService _reportingService;

        public ReportsController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        
        /// Returns customer statistics.
        
        [HttpGet("customer-summary")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetCustomerSummary()
        {
            var result = await _reportingService.GetCustomerSummaryAsync();
            return Ok(result);
        }

        
        /// Returns recently synchronized customers.
        
        [HttpGet("recent-syncs")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetRecentSyncs(int days = 30)
        {
            var result = await _reportingService.GetRecentSyncsAsync(days);
            return Ok(result);
        }

        
        /// Returns soft deleted customers.
        
        [HttpGet("deleted-customers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDeletedCustomers()
        {
            var result = await _reportingService.GetDeletedCustomersAsync();
            return Ok(result);
        }

        
        /// Returns duplicate customer records.
        
        [HttpGet("duplicates")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDuplicateCustomers()
        {
            var result = await _reportingService.GetDuplicateCustomersAsync();
            return Ok(result);
        }

        
        /// Returns data quality metrics.
        
        [HttpGet("data-quality")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetDataQualityReport()
        {
            var result = await _reportingService.GetDataQualityReportAsync();
            return Ok(result);
        }

        
        /// Returns API usage statistics.
        
        [HttpGet("api-usage")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetApiUsage()
        {
            var result = await _reportingService.GetApiUsageStatisticsAsync();
            return Ok(result);
        }

        
        /// Health report for the system.
        
        [HttpGet("system-health")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSystemHealth()
        {
            var result = await _reportingService.GetSystemHealthAsync();
            return Ok(result);
        }
    }
}