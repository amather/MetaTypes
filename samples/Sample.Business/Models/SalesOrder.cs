using MetaTypes.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sample.Business.Models;

[MetaType]
[Table("SalesOrders")]
public record SalesOrder
{
    public int Id { get; init; }
    public int CustomerId { get; init; }
    public DateTime OrderDate { get; init; }
    public string OrderNumber { get; init; } = default!;
    public OrderStatus Status { get; init; }
    public decimal SubTotal { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal ShippingCost { get; init; }
    public decimal TotalAmount { get; init; }
    public string? Notes { get; init; }
    public DateTime? ShippedDate { get; init; }
    public DateTime? DeliveredDate { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; init; }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}