# Sample.Statics.Repository

## Overview
This sample project demonstrates the **Statics.Repository** generator, which will generate repository classes for static service methods with DbContext integration.

## Project Structure

```
Sample.Statics.Repository/
├── Models/
│   ├── User.cs                     # User entity with [MetaType]
│   └── Order.cs                    # Order entity with [MetaType]
├── DB/
│   └── SampleDbContext.cs          # EF Core DbContext with DbSet<User> and DbSet<Order>
├── Program.cs                      # Demo application
├── metatypes.config.json          # Generator configuration
└── README.md                       # This file
```

## Features

### Entity Models
- **User**: Id, UserName, Email, IsActive, CreatedAt, DisplayName
- **Order**: Id, UserId, Amount, Status, PaymentMethod, CreatedAt, UpdatedAt, IncludeShipping

### DbContext
- **SampleDbContext** in `DB` namespace
- DbSet<User> Users
- DbSet<Order> Orders
- Configured with EF Core 9.0 and SQLite

## Configuration

The `metatypes.config.json` is configured for the Statics.Repository generator:

```json
{
  "EnableDiagnosticFiles": true,
  "GenerateBaseMetaTypes": true,
  "DiscoverCrossAssembly": false,
  "DiscoverMethods": [
    "MetaTypes.Attribute.Syntax"
  ],
  "EnabledVendors": [],
  "VendorConfigs": {}
}
```

**Note**: Currently using `MetaTypes.Attribute.Syntax` for base type generation only. When the Statics.Repository generator is implemented, the configuration will be updated to:

```json
{
  "DiscoverMethods": [
    "MetaTypes.Attribute.Syntax",
    "Statics.Repository"
  ],
  "EnabledVendors": [
    "Statics.Repository"
  ],
  "VendorConfigs": {
    "Statics.Repository": {
      "RequireBaseTypes": true
    }
  }
}
```

## Implementation Status

🚧 **In Development**

The Statics.Repository generator is not yet implemented. When complete, it will:

1. **Discover** static service methods via `Statics.Repository` discovery method
2. **Analyze** methods and classify by entity type (User, Order, Global)
3. **Generate** repository classes:
   - `UserRepository` - Methods for User entity
   - `OrderRepository` - Methods for Order entity
   - `GlobalRepository` - Methods without entity binding
4. **Generate** DI extensions for repository registration

## Expected Generated Code (Future)

### Repository Classes
```csharp
public class UserRepository : IStaticsRepository
{
    // Entity-specific methods
    public Task<ServiceResult<User>> GetUserById(int id) { ... }
    public Task<ServiceResult> UpdateUserStatus(int id, bool isActive) { ... }
}

public class OrderRepository : IStaticsRepository
{
    // Entity-specific methods
    public Task<ServiceResult<Order>> GetOrderById(int id) { ... }
}

public class GlobalRepository : IStaticsRepository
{
    // Global methods
    public Task<ServiceResult> PerformMaintenance() { ... }
}
```

### DI Extensions
```csharp
services.AddMetaTypesSampleStaticsRepositoryStaticsRepositories();

var repos = serviceProvider.GetServices<IStaticsRepository>();
var userRepo = serviceProvider.GetService<UserRepository>();
```

## Running the Sample

```bash
cd samples/Vendor/Statics/Sample.Statics.Repository
dotnet run
```

## Differences from Sample.Statics.ServiceMethod

| Feature | ServiceMethod | Repository |
|---------|---------------|------------|
| **Focus** | Service method metadata | Repository generation |
| **Generator** | Statics.ServiceMethod | Statics.Repository |
| **Has Services** | ✅ Yes | ❌ No |
| **Has DbContext** | ❌ No | ✅ Yes |
| **Has Attributes** | ✅ Yes | ❌ No |
| **Generates** | Metadata classes | Repository classes |

## References

- Main Statics Vendor Documentation: `samples/Vendor/Statics/CLAUDE.md`
- ServiceMethod Sample: `samples/Vendor/Statics/Sample.Statics.ServiceMethod/`
