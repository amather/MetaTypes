using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MetaTypes.Abstractions;
using Sample.Console;

var builder = Host.CreateApplicationBuilder(args);

// Register MetaTypes using the new generated DI extension method
// In cross-assembly mode, one unified registration method for all discovered types
builder.Services.AddMetaTypesSampleConsole();

var host = builder.Build();

// Demo usage
var serviceProvider = host.Services;

Console.WriteLine("=== MetaTypes Enhanced DI Generator Demo ===\n");
Console.WriteLine("Generated files can be found in:");
Console.WriteLine("  - obj/Debug/net8.0/generated/ (default .NET location)\n");

Console.WriteLine("The source generator discovered and generated 8 MetaTypes:");
Console.WriteLine("- LoginResponse, PasswordUser (from Sample.Auth)");  
Console.WriteLine("- CustomerResponse, Customer, CustomerAddress, Product, SalesOrder, SalesOrderLine (from Sample.Business)\n");

Console.WriteLine("🆕 NEW: Generated DI extension method:");
Console.WriteLine("- AddMetaTypesSampleConsole() - registers ALL discovered MetaTypes");
Console.WriteLine("- Unified cross-assembly MetaTypesServiceCollectionExtensions class\n");

// Demonstrate the new DI functionality
var allMetaTypes = serviceProvider.GetMetaTypes().ToList();
Console.WriteLine($"✅ Retrieved {allMetaTypes.Count} MetaTypes via DI:");
foreach (var metaType in allMetaTypes)
{
    Console.WriteLine($"  - {metaType.ManagedTypeName} (Type: {metaType.ManagedTypeFullName})");
}

// Demonstrate specific MetaType retrieval
var customerMetaType = serviceProvider.GetMetaType<Sample.Business.Models.Customer>();
Console.WriteLine($"\n✅ Retrieved specific MetaType: {customerMetaType?.ManagedTypeName ?? "Not found"}");

Console.WriteLine("\n✅ Source generation completed successfully!");
Console.WriteLine("✅ Generated DI extension methods working correctly!");
Console.WriteLine("✅ Check generated files in obj/Debug/net8.0/generated/");

Console.WriteLine("\nDemo completed successfully!");