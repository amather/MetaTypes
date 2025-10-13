using MetaTypes.Abstractions;
using Statics.ServiceBroker.Attributes;

namespace Sample.Statics.Repository.Models;

/// <summary>
/// User entity model for the sample application
/// </summary>
public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? DisplayName { get; set; }
}
