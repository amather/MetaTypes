using Microsoft.EntityFrameworkCore;
using Sample.Business.Models;

namespace Sample.Business.Data;

public class BusinessDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; } = default!;
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<SalesOrder> Orders { get; set; } = default!;
    public DbSet<SalesOrderLine> OrderLines { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("BusinessDb");
    }
}