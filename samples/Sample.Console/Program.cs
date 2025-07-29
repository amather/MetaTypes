using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MetaTypes.Abstractions;
using Sample.Business.Models;
using Sample.Auth.Models;
using Sample.Console.Data;

var builder = Host.CreateApplicationBuilder(args);

// Configure EF Core with SQLite
builder.Services.AddDbContext<SampleDbContext>(options =>
    options.UseSqlite("Data Source=sample.db"));

// Register MetaTypes from different assemblies
builder.Services.AddMetaTypes<Sample.Business.MetaTypes>();
builder.Services.AddMetaTypes<Sample.Auth.MetaTypes>();

var host = builder.Build();

// Ensure database is created
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    Console.WriteLine("Database created/verified successfully.");
}

// Demo usage
var serviceProvider = host.Services;

Console.WriteLine("=== MetaTypes Cross-Assembly Demo ===\n");

// Get MetaType for Customer (from Sample.Business assembly)
var mtCustomer = serviceProvider.GetMetaType<Customer>();
Console.WriteLine($"Customer MetaType:");
Console.WriteLine($"  Name: {mtCustomer.ManagedTypeName}");
Console.WriteLine($"  Namespace: {mtCustomer.ManagedTypeNamespace}");
Console.WriteLine($"  Assembly: {mtCustomer.ManagedTypeAssembly}");
Console.WriteLine($"  Members count: {mtCustomer.Members.Count}");

Console.WriteLine("\n  Customer Members:");
foreach (var member in mtCustomer.Members)
{
    Console.WriteLine($"    {member.MemberName} ({member.MemberType.Name}) - HasSetter: {member.HasSetter}, IsList: {member.IsList}");
}

// Expression-based member finding (preferred - type-safe)
var mtmCustomerName = mtCustomer.FindMember(c => c.Name);
var mtmCustomerEmail = mtCustomer.FindMember(c => c.Email);
if (mtmCustomerName != null)
{
    Console.WriteLine($"\n  Name property: {mtmCustomerName.MemberName} - Type: {mtmCustomerName.MemberType.Name}");
}

// String-based member finding (for demonstration)
var mtmCustomerEmailByString = mtCustomer.FindMember("Email");
if (mtmCustomerEmailByString != null)
{
    Console.WriteLine($"  Email property (via string): {mtmCustomerEmailByString.MemberName}");
}

Console.WriteLine("\n" + new string('=', 50) + "\n");

// Get MetaType for PasswordUser (from Sample.Auth assembly)
var mtPasswordUser = serviceProvider.GetMetaType<PasswordUser>();
Console.WriteLine($"PasswordUser MetaType:");
Console.WriteLine($"  Name: {mtPasswordUser.ManagedTypeName}");
Console.WriteLine($"  Namespace: {mtPasswordUser.ManagedTypeNamespace}");
Console.WriteLine($"  Assembly: {mtPasswordUser.ManagedTypeAssembly}");
Console.WriteLine($"  Members count: {mtPasswordUser.Members.Count}");

// Show attributes if any
var mtmPasswordUserEmail = mtPasswordUser.FindMember("Email");
if (mtmPasswordUserEmail?.Attributes != null && mtmPasswordUserEmail.Attributes.Length > 0)
{
    Console.WriteLine($"\n  Email property attributes:");
    foreach (var attr in mtmPasswordUserEmail.Attributes)
    {
        Console.WriteLine($"    {attr.GetType().Name}");
    }
}

Console.WriteLine("\n" + new string('=', 50) + "\n");

// Demonstrate EfCore MetaType integration
Console.WriteLine("=== EfCore MetaType Integration Demo ===\n");

// Show EfCore-specific metadata for Customer
if (mtCustomer is IMetaTypeEfCore mtCustomerEfCore)
{
    Console.WriteLine($"Customer EfCore Metadata:");
    Console.WriteLine($"  Table Name: {mtCustomerEfCore.TableName}");
}

// Show EfCore-specific metadata for Product
var mtProduct = serviceProvider.GetMetaType<Product>();
if (mtProduct is IMetaTypeEfCore mtProductEfCore)
{
    Console.WriteLine($"\nProduct EfCore Metadata:");
    Console.WriteLine($"  Table Name: {mtProductEfCore.TableName}");
}

// Demonstrate EfCore member metadata (foreign keys)
Console.WriteLine("\nEfCore Member Metadata (Foreign Key Detection):");
var mtmProductCustomerId = mtProduct.FindMember("CustomerId");
if (mtmProductCustomerId is IMetaTypeMemberEfCore mtmProductCustomerIdEfCore)
{
    Console.WriteLine($"  Product.CustomerId - IsForeignKey: {mtmProductCustomerIdEfCore.IsForeignKey}");
}

var mtmProductSalesOrderId = mtProduct.FindMember("SalesOrderId");
if (mtmProductSalesOrderId is IMetaTypeMemberEfCore mtmProductSalesOrderIdEfCore)
{
    Console.WriteLine($"  Product.SalesOrderId - IsForeignKey: {mtmProductSalesOrderIdEfCore.IsForeignKey}");
}

// Show all entities discovered through DbContext
Console.WriteLine("\n" + new string('=', 50) + "\n");
Console.WriteLine("=== Entities Discovered through DbContext ===\n");

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
    var entityTypes = dbContext.Model.GetEntityTypes();
    
    foreach (var entityType in entityTypes)
    {
        Console.WriteLine($"Entity: {entityType.ClrType.Name}");
        Console.WriteLine($"  Table: {entityType.GetTableName()}");
        Console.WriteLine($"  Schema: {entityType.GetSchema() ?? "default"}");
        
        // Try to get MetaType for this entity
        var mtProvider = serviceProvider.GetService<IMetaTypeProvider>();
        if (mtProvider != null)
        {
            var mtEntity = mtProvider.AssemblyMetaTypes
                .FirstOrDefault(mt => mt.ManagedType == entityType.ClrType);
            
            if (mtEntity is IMetaTypeEfCore mtEntityEfCore)
            {
                Console.WriteLine($"  MetaType TableName: {mtEntityEfCore.TableName}");
            }
        }
        Console.WriteLine();
    }
}

// Test partial class extensions
Console.WriteLine("\n" + new string('=', 50) + "\n");
Console.WriteLine("=== Testing Partial Class Extensions ===\n");

// Test that CustomerMetaType now implements IMetaTypeEfCore
if (mtCustomer is IMetaTypeEfCore mtCustomerEfCoreTest)
{
    Console.WriteLine($"✅ CustomerMetaType implements IMetaTypeEfCore");
    Console.WriteLine($"   Table name: {mtCustomerEfCoreTest.TableName}");
}
else
{
    Console.WriteLine($"❌ CustomerMetaType does not implement IMetaTypeEfCore");
}

// Test member extensions
var mtmCustomerId = mtCustomer.FindMember("Id");
if (mtmCustomerId is IMetaTypeMemberEfCore mtmCustomerIdEfCore)
{
    Console.WriteLine($"✅ CustomerMetaTypeMemberId implements IMetaTypeMemberEfCore");
    Console.WriteLine($"   Is foreign key: {mtmCustomerIdEfCore.IsForeignKey}");
}
else
{
    Console.WriteLine($"❌ CustomerMetaTypeMemberId does not implement IMetaTypeMemberEfCore");
}

// Test dynamic property access (GetValue/SetValue)
Console.WriteLine("\n" + new string('=', 50) + "\n");
Console.WriteLine("=== Testing Dynamic Property Access ===\n");

var testCustomer = new Customer
{
    Id = 42,
    Name = "John Doe",
    Email = "john@example.com",
    CreatedAt = DateTime.Now,
    IsActive = true
};

// Test GetValue
if (mtmCustomerId != null && mtmCustomerName != null && mtmCustomerEmail != null)
{
    Console.WriteLine("✅ Testing GetValue:");
    Console.WriteLine($"   Id: {mtmCustomerId.GetValue(testCustomer)}");
    Console.WriteLine($"   Name: {mtmCustomerName.GetValue(testCustomer)}");
    Console.WriteLine($"   Email: {mtmCustomerEmail.GetValue(testCustomer)}");
    
    // Test SetValue
    Console.WriteLine("\n✅ Testing SetValue:");
    mtmCustomerId.SetValue(testCustomer, 99);
    mtmCustomerName.SetValue(testCustomer, "Jane Smith");
    mtmCustomerEmail.SetValue(testCustomer, "jane@example.com");
    
    Console.WriteLine($"   Updated Id: {testCustomer.Id}");
    Console.WriteLine($"   Updated Name: {testCustomer.Name}");
    Console.WriteLine($"   Updated Email: {testCustomer.Email}");
}

Console.WriteLine("\nDemo completed successfully! Generated files can be found in:");
Console.WriteLine("  - samples/Sample.Console/obj/Debug/net8.0/generated/");
Console.WriteLine("  - samples/Sample.Business/obj/Debug/net8.0/generated/");
Console.WriteLine("  - samples/Sample.Auth/obj/Debug/net8.0/generated/");

await host.RunAsync();