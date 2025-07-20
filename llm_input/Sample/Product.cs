
namespace MetaTypes.Sample;

[Table("ABC.Product")]
public class Product
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }

}