using System;
using System.ComponentModel.DataAnnotations;
using MetaTypes.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statics.ServiceBroker.Attributes;

namespace Sample.Statics.ServiceMethod.Services;

/// <summary>
/// Static service class containing order management methods with complex attributes
/// </summary>
[MetaType]
public static class OrderServices
{
    [StaticsServiceMethod(Path = "/orders/{orderId:int}/payment", ResponseType = typeof(bool), Policy = ServicePolicyType.Restricted)]
    public static bool ProcessPayment(
        [Required] int orderId, 
        [Range(0.01, 1000000)] decimal amount,
        string paymentMethod = "Credit")
    {
        return amount > 0;
    }

    [StaticsServiceMethod(Path = "/orders/{orderId:int}/events", EntityGlobal = false)]
    public static void LogOrderEvent(
        int orderId, 
        string eventType, 
        [MaxLength(500)] string? description = null)
    {
        // Simulate logging
    }

    [StaticsServiceMethod(Path = "/orders/{orderId:int}/total", Entity = typeof(OrderServices), ResponseType = typeof(decimal))]
    public static async Task<decimal> CalculateOrderTotalAsync(
        [Required] int orderId,
        [FromServices] ILogger logger,
        [FromServices] IServiceProvider services,
        bool includeShipping = true,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Calculating total for order {OrderId}", orderId);
        await Task.Delay(50, cancellationToken);
        return includeShipping ? 125.50m : 100.00m;
    }

    // Internal method should not be discovered
    internal static string FormatOrderNumber(int orderId)
    {
        return $"ORD-{orderId:D6}";
    }
}