namespace DemoERPApi.Models;


/// DTO for customer summary report statistics.

public class CustomerSummaryReportDto
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int DeletedCustomers { get; set; }
}