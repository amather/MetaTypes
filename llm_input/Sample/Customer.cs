namespace MetaTypes.Sample;


[Table("ABC.Customer")]
public class Customer
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }

    public List<SalesOrder>? SalesOrders { get; set; }
}