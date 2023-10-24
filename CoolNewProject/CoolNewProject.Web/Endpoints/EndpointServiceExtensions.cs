namespace CoolNewProject.Web.Endpoints; 

/// <summary>
/// Helper methods to structure code around the new asp .net core minimal API.
/// This replaces the controller based anti-pattern (https://ardalis.com/mvc-controllers-are-dinosaurs-embrace-api-endpoints/)
/// The default extension methods around minimal API make it kind of hard to properly structure the endpoints.
/// With this helper class structuring gets a lot easier and follows the convention-over-configuration pattern.
/// </summary>
public static class EndpointServiceExtensions {
    /// <summary>
    /// Adds all endpoints (IEndpoint) and endpoint providers (IEndpointProvider) to DI
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddEndpoints(this IServiceCollection services) {
        return services.AddImplementations(new HashSet<Type> { typeof(IEndpoint), typeof(IEndpointProvider) });
    }

    /// <summary>
    /// Helper method to register all implementations of certain interface types in all assemblies.
    /// This could be simplified with an assembly scanner like Scrutor (https://github.com/khellang/Scrutor).
    /// </summary>
    /// <param name="services"></param>
    /// <param name="interfaceTypes"></param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    private static IServiceCollection AddImplementations(this IServiceCollection services, 
        IReadOnlySet<Type> interfaceTypes, ServiceLifetime? lifetime = null) {
        lifetime ??= ServiceLifetime.Singleton;
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Select(t => {
                if (t.IsInterface) return null;
                foreach (var @interface in t.GetInterfaces()) {
                    if (interfaceTypes.Contains(@interface)) {
                        return new { InterfaceType = @interface, ImplementationType = t };
                    }
                }
                return null;
            })
            .Where(t => t != null);

        foreach (var type in types) {
            services.Add(new ServiceDescriptor(type!.ImplementationType, type.ImplementationType, lifetime.Value));
            services.Add(new ServiceDescriptor(type.InterfaceType, 
                sp => sp.GetRequiredService(type.ImplementationType), 
                lifetime.Value));
        }
        return services;
    }

    /// <summary>
    /// Maps all nested types endpoints of rootType registered in DI
    /// </summary>
    /// <param name="builder"></param>
    public static IEndpointRouteBuilder MapEndpoints<TRootType>(this IEndpointRouteBuilder builder) {
        return MapEndpoints(builder, typeof(TRootType));
    }
    
    public static IEndpointRouteBuilder MapEndpoint<T>(this IEndpointRouteBuilder builder) where T: IEndpoint {
        var endpoint = builder.ServiceProvider.GetRequiredService<T>();
        endpoint.AddRoute(builder);
        return builder;
    }
    
    public static IEndpointRouteBuilder MapEndpoint(this IEndpointRouteBuilder builder, Type endpointType) {
        if (builder.ServiceProvider.GetRequiredService(endpointType) is not IEndpoint endpoint) {
            throw new InvalidOperationException($"Invalid endpoint type specified {endpointType}");
        }
        endpoint.AddRoute(builder);
        return builder;
    }

    /// <summary>
    /// Maps all nested types endpoints of rootType registered in DI
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="rootType">The root type to filter nested types for</param>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder, Type? rootType = null) {
        var nestedTypes = rootType?.GetNestedTypes().ToHashSet();
        var endpoints = builder.ServiceProvider.GetServices<IEndpoint>();
        foreach (var endpoint in endpoints) {
            if (nestedTypes != null && !nestedTypes.Contains(endpoint.GetType())) continue;
            endpoint.AddRoute(builder);
        }
        return builder;
    }

    /// <summary>
    /// Calls all IEndpointProvider instances registered in DI.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapEndpointProviders(this WebApplication app) {
        var endpointProviders = app.Services.GetServices<IEndpointProvider>();
        IEndpointRouteBuilder builder = app;
        foreach (var endpointProvider in endpointProviders) {
            builder = endpointProvider.MapEndpoints(builder);
        }
        return builder;
    }
}