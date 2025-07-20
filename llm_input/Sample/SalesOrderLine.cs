namespace MetaTypes.Sample;

[Table("ABC.SalesOrderLine")]
public class SalesOrderLine
{
    [Key]
    public int Id { get; set; }

    public int SalesOrderId { get; set; }   

    public SalesOrderLineType LineType { get; set; } 

    [ForeignKey(namemof(SalesOrderId))]
    public SalesOrder? SalesOrder { get; set; }
}


[MetaType]
public enum SalesOrderLineType
{
    Product,
    Service
}