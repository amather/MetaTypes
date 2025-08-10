using Microsoft.EntityFrameworkCore;
using Sample.Auth.Models;
using Sample.Business.Models;
using Sample.EfCore.Infrastructure.Entities;

namespace Sample.EfCore.LocalOnly.Data;

public class LocalDbContext : DbContext
{
    public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options)
    {
    }

    public DbSet<MailLog> MailLogs { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<PasswordUser> PasswordUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure MailLog
        modelBuilder.Entity<MailLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SmtpHost).IsRequired();
            entity.Property(e => e.From).IsRequired();
            entity.Property(e => e.To).IsRequired();
            entity.Property(e => e.Subject).IsRequired();
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>();
        });

        // Configure Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });

        // Configure PasswordUser
        modelBuilder.Entity<PasswordUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
        });
    }
}