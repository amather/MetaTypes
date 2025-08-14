using System.ComponentModel.DataAnnotations;
using MetaTypes.Abstractions;

namespace Sample.EfCore.SingleProject.Models;

// Now has [MetaType] attribute - should be discovered via DbContext scanning
[MetaType]
public class TestEntity
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}