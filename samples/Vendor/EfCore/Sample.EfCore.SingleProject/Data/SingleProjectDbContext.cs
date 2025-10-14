using Microsoft.EntityFrameworkCore;
using Sample.EfCore.SingleProject.Models;

namespace Sample.EfCore.SingleProject.Data;

public class SingleProjectDbContext : DbContext
{
    public SingleProjectDbContext(DbContextOptions<SingleProjectDbContext> options) : base(options)
    {
    }

    public DbSet<LocalEntity> LocalEntities { get; set; }
    public DbSet<TestEntity> TestEntities { get; set; }
    public DbSet<OrderLine> OrderLines { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure LocalEntity (single key: Id)
        modelBuilder.Entity<LocalEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure TestEntity (single key: Id)
        modelBuilder.Entity<TestEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        // Configure OrderLine (composite key: OrderId + LineNumber)
        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.LineNumber });
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
        });

        // Configure AuditLog (no key entity - for demonstration)
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasNoKey();
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.User).IsRequired().HasMaxLength(100);
        });
    }
}
