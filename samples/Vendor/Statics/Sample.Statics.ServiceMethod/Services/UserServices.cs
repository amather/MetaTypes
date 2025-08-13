using MetaTypes.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.Statics.ServiceMethod.Attributes;
using Statics.ServiceBroker.Attributes;

namespace Sample.Statics.ServiceMethod.Services;

/// <summary>
/// Static service class containing user management methods
/// </summary>
[MetaType]
public static class UserServices
{
    [StaticsServiceMethod(Path = "/users/{userId:int}", Policy = ServicePolicyType.Anonymous)]
    public static string GetUserById(int userId)
    {
        return $"User {userId}";
    }

    [StaticsServiceMethod(Path = "/users", EntityGlobal = true, ResponseType = typeof(bool))]
    public static bool CreateUser(string userName, string email, bool isActive = true)
    {
        // Simulate user creation
        return !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(email);
    }

    [StaticsServiceMethod(Path = "/users/{userId:int}/status", Entity = typeof(UserServices))]
    public static void UpdateUserStatus(int userId, bool isActive, string? reason = null)
    {
        // Simulate status update
    }

    [StaticsServiceMethod(Path = "/users/{userId:int}/detailed", ResponseType = typeof(string), Policy = ServicePolicyType.Restricted)]
    public static async Task<string> GetUserWithLoggingAsync(
        int userId, 
        [FromServices] ILogger logger,
        [FromServices] IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting user {UserId}", userId);
        await Task.Delay(100, cancellationToken);
        return $"User {userId} (logged)";
    }

    [StaticsServiceMethod(Path = "/users/validate", EntityGlobal = true, ResponseType = typeof(bool), Policy = ServicePolicyType.Anonymous)] 
    public static bool ValidateUserData(
        string username,
        string email,
        [FromServices] ILogger logger,
        int maxRetries = 3)
    {
        logger.LogDebug("Validating user data for {Username}", username);
        return !string.IsNullOrEmpty(username) && email.Contains('@');
    }

    // Example showing CONSTRUCTOR ARGUMENTS vs NAMED ARGUMENTS
    [CustomService("UserMigration", 100, Description = "Migrates user data", IsEnabled = true, ReturnType = typeof(string))]
    [StaticsServiceMethod(Path = "/users/migrate", Policy = ServicePolicyType.Restricted)]
    public static string MigrateUserData(int batchSize)
    {
        return $"Migrated {batchSize} users";
    }

    // This method should not be discovered (no attribute)
    public static string InternalHelper(string data)
    {
        return data.ToUpper();
    }
}