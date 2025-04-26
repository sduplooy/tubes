using System;
using Microsoft.Extensions.DependencyInjection;

namespace Tubes;

public static class ServiceCollectionExtensions
{
    public static void AddFiltersFromAssemblyContaining<T>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        var discoveredTypes = typeof(T).Assembly.GetTypes();
        
        foreach (var discoveredType in discoveredTypes)
        {
            if (discoveredType.IsAbstract || discoveredType.IsInterface)
                continue;

            var interfaces = discoveredType.GetInterfaces();

            foreach (var type in interfaces)
            {
                if (!type.IsGenericType) 
                    continue;
                
                var definition = type.GetGenericTypeDefinition();
                if (definition == typeof(IFilter<>) || definition == typeof(IAsyncFilter<>))
                    services.Add(new ServiceDescriptor(type, discoveredType, lifetime));
            }
        }
    }
}