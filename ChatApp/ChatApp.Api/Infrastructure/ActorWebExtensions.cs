using ChatApp.Application;

namespace ChatApp.Api.Infrastructure;

public static class ActorWebExtensions {
    public static IApplicationBuilder UseWebServiceScope(this IApplicationBuilder app) {
        ArgumentNullException.ThrowIfNull(app);
        return app.Use(async (context, next) => {
            var actorServiceProvider = context.RequestServices.GetRequiredService<ActorServiceScopeProvider>();
            string requestId = context.TraceIdentifier;
            using var _ = actorServiceProvider.SetServiceProvider(requestId, context.RequestServices);
            await next();
        });
    }
}
