using System.ComponentModel.DataAnnotations.Schema;

namespace DemoERPApi.Models;

[Table("SyncLogs")]
public class SyncLogs
{
    public int LogId { get; set; }

    public string CRMCustomerID { get; set; } = string.Empty;

    public string Operation { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? Message { get; set; }

    public string? Username { get; set; }

    public string? RequestId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? ExecutionTimeMs { get; set; }
}