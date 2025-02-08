namespace ChatApp.Api.Infrastructure;

public static class EndpointRouteBuilderExtensions {
    public static string GetPath(this EndpointGroupBase group) {
        return $"/{group.GroupName.ToLowerInvariant()}";
    }

    public static RouteHandlerBuilder WithDefaultMetadata(this RouteHandlerBuilder builder) {
        builder.Add(x => {
            string? endpointName = x.DisplayName?.Split(" => ").LastOrDefault();
            if (endpointName == null) return;
            // WithName -> openapi operationId
            x.Metadata.Add(new EndpointNameMetadata(endpointName));
            x.Metadata.Add(new RouteNameMetadata(endpointName));
        });
        return builder;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app) {
        var endpointGroupType = typeof(EndpointGroupBase);

        var assembly = typeof(EndpointGroupBase).Assembly;

        var endpointGroupTypes = assembly.GetExportedTypes()
            .Where(t => t.IsSubclassOf(endpointGroupType));
        foreach (var type in endpointGroupTypes) {
            object instanceObj = ActivatorUtilities.CreateInstance(app.ServiceProvider, type);
            if (instanceObj is not EndpointGroupBase instance) {
                continue;
            }
            app.MapEndpoint(instance);
        }

        return app;
    }

    private static IEndpointRouteBuilder MapEndpoint(this IEndpointRouteBuilder routeBuilder, EndpointGroupBase group) {
        var builder = routeBuilder
            .MapGroup(group.GroupName.ToLowerInvariant())
            .WithTags(group.GroupName)
            .WithOpenApi();
        group.Map(builder);
        return builder;
    }
}
