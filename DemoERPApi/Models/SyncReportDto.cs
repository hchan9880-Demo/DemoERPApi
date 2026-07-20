namespace DemoERPApi.Models;


/// DTO for synchronization report data.

public class SyncReportDto
{
    public int CustomerId { get; set; }
    public string CRMCustomerID { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SyncDate { get; set; }
}