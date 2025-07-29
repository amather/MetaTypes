using MetaTypes.Abstractions;

namespace Sample.Business.Models;

[MetaType]
public class CustomerAddress
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}