using MetaTypes.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sample.Business.Models;

[MetaType]
[Table("SalesOrderLines")]
public record SalesOrderLine
{
    public int Id { get; init; }
    public int SalesOrderId { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = default!;
    public string ProductSku { get; init; } = default!;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
    public decimal? DiscountAmount { get; init; }
    public decimal? DiscountPercentage { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; init; }
}