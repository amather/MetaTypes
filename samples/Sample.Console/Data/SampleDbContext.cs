using Microsoft.EntityFrameworkCore;
using Sample.Business.Models;
using Sample.Auth.Models;

namespace Sample.Console.Data;

public class SampleDbContext : DbContext
{
    // Business entities
    public DbSet<Customer> Customers { get; set; } = default!;
    public DbSet<CustomerAddress> CustomerAddresses { get; set; } = default!;
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<SalesOrder> SalesOrders { get; set; } = default!;
    public DbSet<SalesOrderLine> SalesOrderLines { get; set; } = default!;
    
    // Auth entities
    public DbSet<PasswordUser> Users { get; set; } = default!;

    public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure relationships and constraints
        modelBuilder.Entity<CustomerAddress>()
            .HasOne<Customer>()
            .WithMany(c => c.Addresses)
            .HasForeignKey(ca => ca.CustomerId);
            
        modelBuilder.Entity<SalesOrder>()
            .HasOne<Customer>()
            .WithMany()
            .HasForeignKey(so => so.CustomerId);
            
        modelBuilder.Entity<SalesOrderLine>()
            .HasOne<SalesOrder>()
            .WithMany()
            .HasForeignKey(sol => sol.SalesOrderId);
            
        modelBuilder.Entity<SalesOrderLine>()
            .HasOne<Product>()
            .WithMany()
            .HasForeignKey(sol => sol.ProductId);
    }
}