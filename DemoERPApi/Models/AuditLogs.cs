using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoERPApi.Models;


/// Audit log for tracking data changes (CREATE, UPDATE, DELETE).

[Table("AuditLogs")]
public class AuditLogs
{
    [Key]
    public int AuditId { get; set; }

    [MaxLength(100)]
    public string EntityName { get; set; } = string.Empty;

    [MaxLength(400)]
    public string EntityId { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    [MaxLength(100)]
    public string ChangedBy { get; set; } = string.Empty;

    [MaxLength(800)]
    public string? RequestId { get; set; }

    public DateTime ChangedDate { get; set; }
}