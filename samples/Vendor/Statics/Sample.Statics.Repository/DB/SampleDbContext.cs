using Microsoft.EntityFrameworkCore;
using Sample.Statics.Repository.Models;
using Statics.ServiceBroker.Attributes;

namespace Sample.Statics.Repository.DB;

/// <summary>
/// Sample DbContext for the Statics.Repository sample
/// </summary>
[StaticsRepositoryProvider]
public class SampleDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;

    [StaticsRepositoryIgnore]
    public DbSet<AuditLog> AuditLog { get; set; } = null!;

    public SampleDbContext(DbContextOptions<SampleDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
        });
    }
}
