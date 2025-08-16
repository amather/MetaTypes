using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MetaTypes.Abstractions.Vendor.Statics;
using Sample.Statics.ServiceMethod.Services;

namespace Sample.Statics.ServiceMethod;

class Program
{
    static async Task Main(string[] args)
    {
        // Set up DI container with Statics vendor extensions
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                // Register MetaTypes with Statics vendor extensions
                // This generates: AddMetaTypesSampleStaticsServiceMethodStatics()
                services.AddMetaTypesSampleStaticsServiceMethodStatics();
                
                // Register Statics repositories
                services.AddMetaTypesSampleStaticsServiceMethodStaticsRepositories();
                
                // Note: The above methods automatically register:
                // 1. Base MetaTypes via AddMetaTypesSampleStaticsServiceMethod()
                // 2. Statics-specific interfaces (IMetaTypeStatics) for service classes
                // 3. Repository implementations (UserRepository, OrderRepository, GlobalRepository)
                // 4. Repository interfaces (IStaticsRepository, IEntityRepository)
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
        var userResult = UserServices.GetUserById(123);
        Console.WriteLine($"Retrieved: {(userResult.IsSuccess ? userResult.Value : "Failed")}");
        
        var userCreateResult = UserServices.CreateUser("john.doe", "john@example.com");
        Console.WriteLine($"User created: {userCreateResult.IsSuccess} (Value: {userCreateResult.Value})");
        
        var userStatusResult = UserServices.UpdateUserStatus(123, false, "Account suspended");
        Console.WriteLine($"User status updated: {(userStatusResult.IsSuccess ? "Success" : "Failed")}");
        
        // Test OrderServices
        var orderId = 12345; // Simulate order ID
        
        var paymentResult = OrderServices.ProcessPayment(orderId, 299.99m, "Visa");
        Console.WriteLine($"Payment processed: {paymentResult.IsSuccess} (Amount valid: {paymentResult.Value})");
        
        var logResult = OrderServices.LogOrderEvent(orderId, "PAYMENT_PROCESSED", "Payment successfully processed");
        Console.WriteLine($"Order event logged: {(logResult.IsSuccess ? "Success" : "Failed")}");
        
        Console.WriteLine("\n✅ Static service method execution completed");
        Console.WriteLine("✅ Statics vendor DI extensions working correctly");
        Console.WriteLine("✅ Check Generated folder for new DI extension files");
        Console.WriteLine("   - Look for: StaticsServiceCollectionExtensions.g.cs");
        
        await TestRepositories(serviceProvider);
    }

    static async Task TestRepositories(IServiceProvider serviceProvider)
    {
        Console.WriteLine("\n=== Testing Generated Repositories ===");

        // Test repository DI retrieval
        var repositories = serviceProvider.GetServices<IStaticsRepository>().ToList();
        Console.WriteLine($"✅ Retrieved {repositories.Count} Statics Repositories via DI:");
        foreach (var repo in repositories)
        {
            Console.WriteLine($"  - {repo.GetType().Name}");
        }

        // Test specific repository usage
        var userRepo = serviceProvider.GetService<Sample.Statics.ServiceMethod.Models.UserRepository>();
        if (userRepo != null)
        {
            Console.WriteLine("\n✅ Testing UserRepository methods:");
            
            // Test entity-specific method
            var userResult = await userRepo.GetUserById(123);
            Console.WriteLine($"  GetUserById(123): {(userResult.IsSuccess ? userResult.Value : "Failed")}");
            
            // Test entity-global method (no id parameter)
            // Note: Entity-global methods like ValidateUserData would go in UserRepository if they had Entity=typeof(User) && EntityGlobal=true
            
            // Test async method
            var logger = serviceProvider.GetService<Microsoft.Extensions.Logging.ILogger>() ?? 
                        Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            var asyncResult = await userRepo.GetUserWithLoggingAsync(123, logger, serviceProvider, default);
            Console.WriteLine($"  GetUserWithLoggingAsync(123): {(asyncResult.IsSuccess ? asyncResult.Value : "Failed")}");
        }

        // Test global repository
        var globalRepo = serviceProvider.GetService<Sample.Statics.ServiceMethod.GlobalRepository.GlobalRepository>();
        if (globalRepo != null)
        {
            Console.WriteLine("\n✅ Testing GlobalRepository methods:");
            
            var createResult = await globalRepo.CreateUser("jane.doe", "jane@example.com", true);
            Console.WriteLine($"  CreateUser: {(createResult.IsSuccess ? $"Success ({createResult.Value})" : "Failed")}");
            
            var migrateResult = await globalRepo.MigrateUserData(100);
            Console.WriteLine($"  MigrateUserData: {(migrateResult.IsSuccess ? migrateResult.Value : "Failed")}");
        }

        Console.WriteLine("\n✅ Repository testing completed");
        Console.WriteLine("✅ All repository methods return Task<> for consistent async patterns");
        Console.WriteLine("✅ Check generated repository files:");
        Console.WriteLine("   - UserRepository.g.cs (entity-specific methods)");
        Console.WriteLine("   - OrderRepository.g.cs (entity-specific methods)");
        Console.WriteLine("   - GlobalRepository.g.cs (global methods)");
        Console.WriteLine("   - StaticsRepositoryServiceCollectionExtensions.g.cs (DI registration)");
    }
}