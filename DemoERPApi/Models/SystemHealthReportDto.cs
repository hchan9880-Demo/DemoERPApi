namespace DemoERPApi.Models;


/// DTO for system health status report.

public class SystemHealthReportDto
{
    public string ApiStatus { get; set; } = string.Empty;
    public string DatabaseStatus { get; set; } = string.Empty;
    public DateTime ServerTime { get; set; }
}