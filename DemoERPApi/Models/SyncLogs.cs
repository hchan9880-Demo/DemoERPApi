using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoERPApi.Models;

[Table("SyncLogs")]
public class SyncLogs
{
    [Key]
    public int LogId { get; set; }

    // CRMCustomerID column exists in the database and is used throughout the codebase
    // keep it as a string to match other models (Customers.Customer.CRMCustomerID)
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
