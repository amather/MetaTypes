using MetaTypes.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.Statics.ServiceMethod.Attributes;
using Sample.Statics.ServiceMethod.Models;
using Statics.ServiceBroker.Attributes;
using Statics.ServiceResult;

namespace Sample.Statics.ServiceMethod.Services;

/// <summary>
/// Static service class containing user management methods
/// </summary>
public static class UserServices
{
    [StaticsServiceMethod(Path = "/users/{id:int}", Entity = typeof(User), Policy = ServicePolicyType.Anonymous)]
    public static ServiceResult<string> GetUserById(int id)
    {
        return new ServiceResult<string>(200, $"User {id}");
    }

    [StaticsServiceMethod(Path = "/users", EntityGlobal = true, ResponseType = typeof(bool))]
    public static ServiceResult<bool> CreateUser(string userName, string email, bool isActive = true)
    {
        // Simulate user creation
        var success = !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(email);
        return new ServiceResult<bool>(success ? 201 : 400, success);
    }

    [StaticsServiceMethod(Path = "/users/{id:int}/status", Entity = typeof(User))]
    public static ServiceResult UpdateUserStatus(int id, bool isActive, string? reason = null)
    {
        // Simulate status update
        return new ServiceResult(200, message: "User status updated");
    }

    [StaticsServiceMethod(Path = "/users/{id:int}/detailed", Entity = typeof(User), ResponseType = typeof(string), Policy = ServicePolicyType.Restricted)]
    public static async Task<ServiceResult<string>> GetUserWithLoggingAsync(
        int id, 
        [FromServices] ILogger logger,
        [FromServices] IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting user {UserId}", id);
        await Task.Delay(100, cancellationToken);
        return new ServiceResult<string>(200, $"User {id} (logged)");
    }

    [StaticsServiceMethod(Path = "/users/validate", EntityGlobal = true, ResponseType = typeof(bool), Policy = ServicePolicyType.Anonymous)] 
    public static ServiceResult<bool> ValidateUserData(
        string username,
        string email,
        [FromServices] ILogger logger,
        int maxRetries = 3)
    {
        logger.LogDebug("Validating user data for {Username}", username);
        var isValid = !string.IsNullOrEmpty(username) && email.Contains('@');
        return new ServiceResult<bool>(200, isValid);
    }

    // Example showing CONSTRUCTOR ARGUMENTS vs NAMED ARGUMENTS
    [CustomService("UserMigration", 100, Description = "Migrates user data", IsEnabled = true, ReturnType = typeof(string))]
    [StaticsServiceMethod(Path = "/users/migrate", Policy = ServicePolicyType.Restricted)]
    public static ServiceResult<string> MigrateUserData(int batchSize)
    {
        return new ServiceResult<string>(200, $"Migrated {batchSize} users");
    }

    // Test method with invalid return type (for validation testing)
    [StaticsServiceMethod(Path = "/users/test-invalid", EntityGlobal = true)]
    public static string InvalidReturnTypeMethod()
    {
        return "This should fail validation";
    }

    // This method should not be discovered (no attribute)
    public static string InternalHelper(string data)
    {
        return data.ToUpper();
    }
}