namespace DemoERPApi.Models
{
    public class ApiUsageReportDto
    {
        public int TotalRequests { get; set; }

        public int SuccessfulRequests { get; set; }

        public int FailedRequests { get; set; }
    }
}