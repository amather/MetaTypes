using MetaTypes.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sample.Business.Models;

[MetaType]
[Table("Products")]
public record Product
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
    public decimal Price { get; init; }
    public string Category { get; init; } = default!;
    public string Sku { get; init; } = default!;
    public int StockQuantity { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; init; }
}