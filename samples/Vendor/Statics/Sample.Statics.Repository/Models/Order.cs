using MetaTypes.Abstractions;

namespace Sample.Statics.Repository.Models;

/// <summary>
/// Order entity model for the sample application
/// </summary>
[MetaType]
public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IncludeShipping { get; set; } = true;
}
