using Microsoft.Extensions.DependencyInjection;

namespace MetaTypes.Abstractions;

public static class ServiceProviderExtensions
{
    public static IMetaType<T> GetMetaType<T>(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<IMetaType<T>>();
    }
    
    public static IMetaType GetMetaType(this IServiceProvider serviceProvider, Type type)
    {
        var serviceType = typeof(IMetaType<>).MakeGenericType(type);
        return (IMetaType)serviceProvider.GetRequiredService(serviceType);
    }
}