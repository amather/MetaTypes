using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MetaTypes.Abstractions.Vendor.Statics;
using Sample.Statics.ServiceMethod.Services;

namespace Sample.Statics.ServiceMethod;

class Program
{
    static void Main(string[] args)
    {
        // Set up DI container with Statics vendor extensions
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                // Register MetaTypes with Statics vendor extensions
                // This generates: AddMetaTypesSampleStaticsServiceMethodStatics()
                services.AddMetaTypesSampleStaticsServiceMethodStatics();
                
                // Note: The above method automatically registers:
                // 1. Base MetaTypes via AddMetaTypesSampleStaticsServiceMethod()
                // 2. Statics-specific interfaces (IMetaTypeStatics) for service classes
            })
            .Build();
        
        using var scope = host.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        
        Console.WriteLine("=== Statics ServiceMethod Enhanced DI Demo ===\n");
        
        // Demonstrate Statics vendor-specific DI retrieval
        var staticsMetaTypes = serviceProvider.GetStaticsMetaTypes().ToList();
        
        Console.WriteLine($"✅ Retrieved {staticsMetaTypes.Count} Statics MetaTypes via vendor-specific DI:");
        foreach (var staticsType in staticsMetaTypes)
        {
            Console.WriteLine($"  - {((global::MetaTypes.Abstractions.IMetaType)staticsType).ManagedTypeName}:");
            Console.WriteLine($"    Service Methods: {staticsType.ServiceMethods.Count}");
            foreach (var method in staticsType.ServiceMethods)
            {
                Console.WriteLine($"      • {method.MethodName}({string.Join(", ", method.Parameters.Select(p => $"{p.ParameterType.Name} {p.ParameterName}"))}) -> {method.ReturnType.Name}");
            }
        }
        
        // Demonstrate specific Statics MetaType retrieval
        var userServicesMetaType = serviceProvider.GetStaticsMetaType(typeof(UserServices));
        if (userServicesMetaType != null)
        {
            Console.WriteLine($"\n✅ Retrieved specific Statics MetaType: {((global::MetaTypes.Abstractions.IMetaType)userServicesMetaType).ManagedTypeName}");
            Console.WriteLine($"   Service Methods Count: {userServicesMetaType.ServiceMethods.Count}");
            
            var firstMethod = userServicesMetaType.ServiceMethods.FirstOrDefault();
            if (firstMethod != null)
            {
                Console.WriteLine($"   First Method: {firstMethod.MethodName} with {firstMethod.MethodAttributes.Count} attributes");
            }
        }
        
        Console.WriteLine("\n=== Testing Static Service Methods ===");
        
        // Test UserServices
        var user = UserServices.GetUserById(123);
        Console.WriteLine($"Retrieved: {user}");
        
        var userCreated = UserServices.CreateUser("john.doe", "john@example.com");
        Console.WriteLine($"User created: {userCreated}");
        
        UserServices.UpdateUserStatus(123, false, "Account suspended");
        Console.WriteLine("User status updated");
        
        // Test OrderServices
        var orderId = 12345; // Simulate order ID
        
        var paymentProcessed = OrderServices.ProcessPayment(orderId, 299.99m, "Visa");
        Console.WriteLine($"Payment processed: {paymentProcessed}");
        
        OrderServices.LogOrderEvent(orderId, "PAYMENT_PROCESSED", "Payment successfully processed");
        Console.WriteLine("Order event logged");
        
        Console.WriteLine("\n✅ Static service method execution completed");
        Console.WriteLine("✅ Statics vendor DI extensions working correctly");
        Console.WriteLine("✅ Check Generated folder for new DI extension files");
        Console.WriteLine("   - Look for: StaticsServiceCollectionExtensions.g.cs");
    }
}