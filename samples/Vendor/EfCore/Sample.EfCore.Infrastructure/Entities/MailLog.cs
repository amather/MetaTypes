using System;
using System.ComponentModel.DataAnnotations.Schema;
using MetaTypes.Abstractions;

namespace Sample.EfCore.Infrastructure.Entities;

[MetaType]
[Table("MailLogs")]
public class MailLog
{
    public int Id { get; set; }
    
    // SMTP Server Configuration
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public bool UseSsl { get; set; }
    
    // Authentication
    public string? Username { get; set; }
    
    // Email Fields
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; }
    
    // Status Tracking
    public EmailStatus Status { get; set; }
    public string? LastServerResponse { get; set; }
    public int RetryCount { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public DateTime? SentAt { get; set; }
    
    // Additional Metadata
    public string? MessageId { get; set; }
    public string? ReplyTo { get; set; }
    public int? Priority { get; set; }
    public string? Headers { get; set; } // JSON serialized additional headers
}