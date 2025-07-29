using MetaTypes.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sample.Auth.Models;

[MetaType]
[Table("Users")]
public class PasswordUser
{
    public int Id { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public string? Salt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? LastLoginAt { get; set; }
    
    public bool IsLocked { get; set; }
    
    public int FailedLoginAttempts { get; set; }
}