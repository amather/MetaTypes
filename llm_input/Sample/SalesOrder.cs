
namespace MetaTypes.Sample;

[Table("ABC.SalesOrder")]
public class SalesOrder
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }

    public int CustomerId { get; set; }


    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }

    public List<SalesOrderLine>? Lines { get; set; }
}