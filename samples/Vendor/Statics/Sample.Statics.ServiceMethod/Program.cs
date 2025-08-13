using System;
using Sample.Statics.ServiceMethod.Services;

namespace Sample.Statics.ServiceMethod;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Statics ServiceMethod sample - Testing static service methods");
        
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
        
        Console.WriteLine();
        Console.WriteLine("Check the Generated folder for MetaTypes generated files");
        Console.WriteLine("Generated files should include static service method metadata");
    }
}