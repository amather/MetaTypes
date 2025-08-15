using System;
using System.ComponentModel.DataAnnotations;
using MetaTypes.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.Statics.ServiceMethod.Models;
using Statics.ServiceBroker.Attributes;

namespace Sample.Statics.ServiceMethod.Services;

/// <summary>
/// Static service class containing order management methods with complex attributes
/// </summary>
public static class OrderServices
{
    [StaticsServiceMethod(Path = "/orders/{id:int}/payment", Entity = typeof(Order), ResponseType = typeof(bool), Policy = ServicePolicyType.Restricted)]
    public static bool ProcessPayment(
        [Required] int id, 
        [Range(0.01, 1000000)] decimal amount,
        string paymentMethod = "Credit")
    {
        return amount > 0;
    }

    [StaticsServiceMethod(Path = "/orders/{id:int}/events", Entity = typeof(Order))]
    public static void LogOrderEvent(
        int id, 
        string eventType, 
        [MaxLength(500)] string? description = null)
    {
        // Simulate logging
    }

    [StaticsServiceMethod(Path = "/orders/{id:int}/total", Entity = typeof(Order), ResponseType = typeof(decimal))]
    public static async Task<decimal> CalculateOrderTotalAsync(
        [Required] int id,
        [FromServices] ILogger logger,
        [FromServices] IServiceProvider services,
        bool includeShipping = true,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Calculating total for order {OrderId}", id);
        await Task.Delay(50, cancellationToken);
        return includeShipping ? 125.50m : 100.00m;
    }

    // Internal method should not be discovered
    internal static string FormatOrderNumber(int id)
    {
        return $"ORD-{id:D6}";
    }
}