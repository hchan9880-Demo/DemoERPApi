namespace DemoERPApi.Models
{
    public class CustomerSummaryReport
    {
        public int TotalCustomers { get; set; }

        public int ActiveCustomers { get; set; }

        public int DeletedCustomers { get; set; }
    }



    public class SyncReport
    {
        public string CRMCustomerID { get; set; }

        public string Status { get; set; }

        public DateTime SyncDate { get; set; }
    }



    public class DuplicateCustomerReport
    {
        public string CRMCustomerID { get; set; }

        public string Email { get; set; }

        public string Reason { get; set; }
    }



    public class ApiUsageReport
    {
        public int TotalRequests { get; set; }

        public int SuccessfulRequests { get; set; }

        public int FailedRequests { get; set; }

        public double SuccessRate =>
            TotalRequests == 0
            ? 0
            : (double)SuccessfulRequests / TotalRequests * 100;
    }



    public class SystemHealthReport
    {
        public string ApiStatus { get; set; }

        public string DatabaseStatus { get; set; }

        public DateTime CheckedAt { get; set; }
    }
}