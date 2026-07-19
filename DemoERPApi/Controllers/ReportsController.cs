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

        /// <summary>
        /// Returns customer statistics.
        /// </summary>
        [HttpGet("customer-summary")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetCustomerSummary()
        {
            var result = await _reportingService.GetCustomerSummaryAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns recently synchronized customers.
        /// </summary>
        [HttpGet("recent-syncs")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetRecentSyncs(int days = 30)
        {
            var result = await _reportingService.GetRecentSyncsAsync(days);
            return Ok(result);
        }

        /// <summary>
        /// Returns soft deleted customers.
        /// </summary>
        [HttpGet("deleted-customers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDeletedCustomers()
        {
            var result = await _reportingService.GetDeletedCustomersAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns duplicate customer records.
        /// </summary>
        [HttpGet("duplicates")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDuplicateCustomers()
        {
            var result = await _reportingService.GetDuplicateCustomersAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns data quality metrics.
        /// </summary>
        [HttpGet("data-quality")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetDataQualityReport()
        {
            var result = await _reportingService.GetDataQualityReportAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns API usage statistics.
        /// </summary>
        [HttpGet("api-usage")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetApiUsage()
        {
            var result = await _reportingService.GetApiUsageStatisticsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Health report for the system.
        /// </summary>
        [HttpGet("system-health")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSystemHealth()
        {
            var result = await _reportingService.GetSystemHealthAsync();
            return Ok(result);
        }
    }
}