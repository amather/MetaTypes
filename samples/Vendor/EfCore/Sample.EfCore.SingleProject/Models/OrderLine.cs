using System.ComponentModel.DataAnnotations;
using MetaTypes.Abstractions;

namespace Sample.EfCore.SingleProject.Models;

/// <summary>
/// Entity with composite key (OrderId + LineNumber)
/// </summary>
[MetaType]
public class OrderLine
{
    [Key]
    public int OrderId { get; set; }

    [Key]
    public int LineNumber { get; set; }

    [Required]
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}
