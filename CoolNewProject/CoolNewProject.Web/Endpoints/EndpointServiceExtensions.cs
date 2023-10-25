using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Options;

namespace CoolNewProject.Web.Endpoints; 

/// <summary>
/// Helper methods to structure code around the new asp .net core minimal API.
/// This replaces the controller based anti-pattern (https://ardalis.com/mvc-controllers-are-dinosaurs-embrace-api-endpoints/)
/// The default extension methods around minimal API make it kind of hard to properly structure the endpoints.
/// With this helper class structuring gets a lot easier and follows the convention-over-configuration pattern.
/// </summary>
public static class EndpointServiceExtensions {
    private static readonly string[] GetVerb = { HttpMethods.Get };
    private static readonly string[] PostVerb = { HttpMethods.Post };
    private static readonly string[] PutVerb = { HttpMethods.Put };
    private static readonly string[] DeleteVerb = { HttpMethods.Delete };
    private static readonly string[] PatchVerb = { HttpMethods.Patch };
    
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

    public static IEndpointConventionBuilder MapGet<T>(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern) where T : IEndpoint
        => endpoints.MapMethods<T>(pattern, GetVerb);
    
    public static IEndpointConventionBuilder MapPut<T>(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern) where T : IEndpoint
        => endpoints.MapMethods<T>(pattern, PutVerb);
    
    public static IEndpointConventionBuilder MapPost<T>(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern) where T : IEndpoint
        => endpoints.MapMethods<T>(pattern, PostVerb);

    public static IEndpointConventionBuilder MapDelete<T>(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern) where T : IEndpoint
        => endpoints.MapMethods<T>(pattern, DeleteVerb);
    
    public static IEndpointConventionBuilder MapPatch<T>(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern) where T : IEndpoint
        => endpoints.MapMethods<T>(pattern, PatchVerb);
    
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static IEndpointConventionBuilder MapMethods<T>(
        this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern,
        IEnumerable<string> httpMethods) where T: IEndpoint {
        ArgumentNullException.ThrowIfNull(httpMethods);
        var endpointType = typeof(T);
        var endpointInterface = endpointType.GetInterfaces().FirstOrDefault(i => typeof(IEndpoint).IsAssignableFrom(i));
        if (endpointInterface == null) throw new NotImplementedException();
        if (typeof(IEndpoint.WithContext).IsAssignableFrom(endpointType)) {
            return endpoints
                .MapMethods(pattern, httpMethods, context => {
                    var contextEndpoint = (IEndpoint.WithContext)context.RequestServices.GetRequiredService<T>();
                    return contextEndpoint.HandleAsync(context);
                });
        }
        return endpoints.MapMethodsFallback<T>(pattern, httpMethods);
    }

    // reflection in here is a one-time fee therefore it influences startup time but not request-response time
    private static IEndpointConventionBuilder MapMethodsFallback<T>(this IEndpointRouteBuilder endpoints,
        [StringSyntax("Route")] string pattern,
        IEnumerable<string> httpMethods) where T: IEndpoint  {
        var endpointType = typeof(T);
        var handleMethod = endpointType
            .GetMethods()
            .MinBy(m => m.Name switch {
                "HandleAsync" => 0,
                "Handle" => 1,
                _ => 100
            });
        if (handleMethod == null) {
            throw new ArgumentException($"Failed to find request handling method for endpoint {endpointType} - please add a HandleAsync or Handle method");
        }
        var routeHandlerOptions = endpoints.ServiceProvider.GetService<IOptions<RouteHandlerOptions>>();
        var throwOnBadRequest = routeHandlerOptions?.Value.ThrowOnBadRequest ?? false;
        
        var routePattern = RoutePatternFactory.Parse(pattern);
        var routeParamNames = new List<string>(routePattern.Parameters.Count);
        routeParamNames.AddRange(routePattern.Parameters.Select(parameter => parameter.Name));
        
        var builder = new RouteEndpointBuilder(null, routePattern, order: 0) {
            ApplicationServices = endpoints.ServiceProvider
        };
        builder.Metadata.Add(handleMethod);
        // Add delegate attributes as metadata before entry-specific conventions but after group conventions.
        var attributes = Attribute.GetCustomAttributes(handleMethod);
        foreach (var attribute in attributes) {
            builder.Metadata.Add(attribute);
        }

        var rdfOptions = new RequestDelegateFactoryOptions {
            EndpointBuilder = builder,
            ServiceProvider = endpoints.ServiceProvider,
            RouteParameterNames = routeParamNames,
            ThrowOnBadRequest = throwOnBadRequest
        };
        var rdfMetadataResult = RequestDelegateFactory.InferMetadata(handleMethod, rdfOptions);

        var requestResult = RequestDelegateFactory.Create(handleMethod, Factory, rdfOptions, metadataResult: rdfMetadataResult);
        
        return endpoints
            .MapMethods(pattern, httpMethods, requestResult.RequestDelegate)
            .WithMetadata(requestResult.EndpointMetadata.ToArray());

        static object Factory(HttpContext context) {
            return context.RequestServices.GetRequiredService<T>();
        }
    }
}