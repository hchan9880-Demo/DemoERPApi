using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoERPApi.Models;


/// Represents a synchronization log entry for customer sync operations.

[Table("SyncLogs")]
public class SyncLogs
{
    [Key]
    public int LogId { get; set; }

    
    /// CRM Customer ID (string to match Customers.CRMCustomerID).
    
    public string CRMCustomerID { get; set; } = string.Empty;

    public int CustomerID { get; set; }

    [ForeignKey(nameof(CustomerID))]
    public Customers Customer { get; set; } = null!;

    public string Operation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? Username { get; set; }
    public string? RequestId { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ExecutionTimeMs { get; set; }
}