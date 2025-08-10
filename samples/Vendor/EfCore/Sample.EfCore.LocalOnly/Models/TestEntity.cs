using System.ComponentModel.DataAnnotations.Schema;
using MetaTypes.Abstractions;

namespace Sample.EfCore.LocalOnly.Models;

[MetaType]
[Table("TestEntities")]
public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}