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
        return services.AddImplementations(new HashSet<Type> { typeof(IEndpointProvider) });
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
        lifetime ??= ServiceLifetime.Scoped;
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
    /// Calls all IEndpointProvider instances registered in DI.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapEndpointProviders(this WebApplication app) {
        // tmp scope
        using var scope = app.Services.CreateScope();
        var endpointProviders = scope.ServiceProvider.GetServices<IEndpointProvider>();
        IEndpointRouteBuilder builder = app;
        foreach (var endpointProvider in endpointProviders) {
            builder = endpointProvider.MapEndpoints(builder);
        }
        return builder;
    }
}