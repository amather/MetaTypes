using Microsoft.EntityFrameworkCore;
using Sample.EfCore.SingleProject.Models;

namespace Sample.EfCore.SingleProject.Data;

public class SingleProjectDbContext : DbContext
{
    public SingleProjectDbContext(DbContextOptions<SingleProjectDbContext> options) : base(options)
    {
    }

    public DbSet<LocalEntity> LocalEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure LocalEntity
        modelBuilder.Entity<LocalEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}