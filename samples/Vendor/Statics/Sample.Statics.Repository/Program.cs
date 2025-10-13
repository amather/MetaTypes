using Microsoft.EntityFrameworkCore;
using Sample.Statics.Repository.DB;
using Sample.Statics.Repository.Models;

namespace Sample.Statics.Repository;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Statics Repository Sample ===");
        Console.WriteLine();
        Console.WriteLine("This sample demonstrates the Statics.Repository generator.");
        Console.WriteLine("The generator will create repositories for static service methods.");
        Console.WriteLine();

        Console.WriteLine("Entity Models:");
        Console.WriteLine("  - User (Id, UserName, Email, IsActive, CreatedAt, DisplayName)");
        Console.WriteLine("  - Order (Id, UserId, Amount, Status, PaymentMethod, CreatedAt, UpdatedAt, IncludeShipping)");
        Console.WriteLine();

        Console.WriteLine("DbContext configuration:");
        Console.WriteLine("  - SampleDbContext in DB namespace");
        Console.WriteLine("  - DbSet<User> Users");
        Console.WriteLine("  - DbSet<Order> Orders");
        Console.WriteLine();

        Console.WriteLine("Configuration (metatypes.config.json):");
        Console.WriteLine("  - Discovery: MetaTypes.Attribute.Syntax (base types only for now)");
        Console.WriteLine("  - Statics.Repository discovery method (not yet implemented)");
        Console.WriteLine("  - Statics.Repository vendor generator (not yet implemented)");
        Console.WriteLine();

        Console.WriteLine("When implemented, the Statics.Repository generator will:");
        Console.WriteLine("  1. Discover static service methods via Statics.Repository discovery");
        Console.WriteLine("  2. Analyze methods and classify by entity type");
        Console.WriteLine("  3. Generate repository classes (UserRepository, OrderRepository, GlobalRepository)");
        Console.WriteLine("  4. Generate DI extensions for repository registration");
        Console.WriteLine();

        Console.WriteLine("Ready for Statics.Repository generator implementation!");
    }
}
