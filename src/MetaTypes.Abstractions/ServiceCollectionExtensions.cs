using Microsoft.Extensions.DependencyInjection;

namespace MetaTypes.Abstractions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMetaTypes<T>(this IServiceCollection services)
        where T : class, IMetaTypeProvider
    {
        var instanceProperty = typeof(T).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        if (instanceProperty?.GetValue(null) is not T provider)
            throw new InvalidOperationException($"Type {typeof(T).Name} must have a static Instance property returning an instance of {typeof(T).Name}");
        
        foreach (var metaType in provider.AssemblyMetaTypes)
        {
            var serviceType = typeof(IMetaType<>).MakeGenericType(metaType.ManagedType);
            services.AddSingleton(serviceType, metaType);
            services.AddSingleton(metaType.GetType(), metaType);
        }
        
        return services;
    }
}