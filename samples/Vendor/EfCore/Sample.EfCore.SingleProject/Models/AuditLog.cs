using System.ComponentModel.DataAnnotations.Schema;
using MetaTypes.Abstractions;

namespace Sample.EfCore.SingleProject.Models;

/// <summary>
/// Entity with no key - demonstrates that no key struct is generated
/// </summary>
[MetaType]
[NotMapped] // Not mapped to avoid EF Core errors about missing key
public class AuditLog
{
    public string Action { get; set; } = string.Empty;

    public string User { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? Details { get; set; }
}
