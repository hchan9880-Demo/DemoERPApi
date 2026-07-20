namespace DemoERPApi.Models;


/// Report models for customer, sync, duplicate, API usage, and system health.

public class CustomerSummaryReport
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int DeletedCustomers { get; set; }
}

public class SyncReport
{
    public string CRMCustomerID { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SyncDate { get; set; }
}

public class DuplicateCustomerReport
{
    public string CRMCustomerID { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class ApiUsageReport
{
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }

    public double SuccessRate =>
        TotalRequests == 0 ? 0 : (double)SuccessfulRequests / TotalRequests * 100;
}

public class SystemHealthReport
{
    public string ApiStatus { get; set; } = string.Empty;
    public string DatabaseStatus { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
}