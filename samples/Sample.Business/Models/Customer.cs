using MetaTypes.Abstractions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sample.Business.Models;

[MetaType]
[Table("Customers")]
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<CustomerAddress> Addresses { get; set; } = [];
}