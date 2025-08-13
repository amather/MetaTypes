using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MetaTypes.Abstractions.Vendor.EfCore;
using Sample.EfCore.SingleProject.Data;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddDbContext<SingleProjectDbContext>(options =>
            options.UseSqlite("Data Source=single.db"));
        
        // Register MetaTypes with EfCore vendor extensions
        // This generates: AddMetaTypesSampleEfCoreSingleProjectEfCore()
        services.AddMetaTypesSampleEfCoreSingleProjectEfCore();
        
        // Note: The above method automatically registers:
        // 1. Base MetaTypes via AddMetaTypesSampleEfCoreSingleProject()
        // 2. EfCore-specific interfaces (IMetaTypeEfCore) for entity types
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SingleProjectDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    
    Console.WriteLine("=== EfCore SingleProject Enhanced DI Demo ===\n");
    
    // Demonstrate EfCore vendor-specific DI retrieval
    var serviceProvider = scope.ServiceProvider;
    var efCoreMetaTypes = serviceProvider.GetEfCoreMetaTypes().ToList();
    
    Console.WriteLine($"✅ Retrieved {efCoreMetaTypes.Count} EfCore MetaTypes via vendor-specific DI:");
    foreach (var efCoreType in efCoreMetaTypes)
    {
        Console.WriteLine($"  - {efCoreType.Name}:");
        Console.WriteLine($"    Table: {efCoreType.TableName}");
        Console.WriteLine($"    Keys: {string.Join(", ", efCoreType.Keys.Select(k => k.Name))}");
    }
    
    // Demonstrate specific EfCore MetaType retrieval
    var localEntityMetaType = serviceProvider.GetEfCoreMetaType<Sample.EfCore.SingleProject.Models.LocalEntity>();
    if (localEntityMetaType != null)
    {
        Console.WriteLine($"\n✅ Retrieved specific EfCore MetaType: {localEntityMetaType.Name}");
        Console.WriteLine($"   Table Name: {localEntityMetaType.TableName}");
        Console.WriteLine($"   Primary Keys: {string.Join(", ", localEntityMetaType.Keys.Select(k => k.Name))}");
    }
    
    Console.WriteLine("\n✅ Database created successfully");
    Console.WriteLine("✅ EfCore vendor DI extensions working correctly");
    Console.WriteLine("✅ Check Generated folder for new DI extension files");
    Console.WriteLine("   - Look for: EfCoreServiceCollectionExtensions.g.cs");
}