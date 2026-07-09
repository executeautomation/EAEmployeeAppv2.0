using System.ComponentModel.DataAnnotations;

namespace EAEmployee.Net8.Models;

public class AuditLog
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Action { get; set; } = string.Empty; // Create, Update, Delete

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [StringLength(256)]
    public string? UserName { get; set; }

    [StringLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>Primary key value of the affected entity.</summary>
    public int? EntityId { get; set; }

    /// <summary>JSON snapshot of the entity state before the change (for Update/Delete).</summary>
    [StringLength(4096)]
    public string? OldValues { get; set; }

    /// <summary>JSON snapshot of the entity state after the change (for Create/Update).</summary>
    [StringLength(4096)]
    public string? NewValues { get; set; }

    /// <summary>Human-readable summary, e.g. "Deleted employee: John Doe".</summary>
    [StringLength(512)]
    public string? Summary { get; set; }
}
