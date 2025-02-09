using ChatApp.Application;
using ChatApp.Common;
using ChatApp.Common.Actors.Abstractions;
using ChatApp.Common.Actors.Local;

namespace ChatApp.Api.Infrastructure;

/// <summary>
/// This shows an example how to override the actor service scope behaviour.
/// In this example the service scope is provided by the web request services.
/// </summary>
public static class ActorWebExtensions {
    public static IServiceCollection AddActorWebServiceScope(this IServiceCollection services) {
        services.AddSingleton<WebActorServiceScopeProvider>();
        services.AddSingleton<IActorServiceScopeProvider>(sp => sp.GetRequiredService<WebActorServiceScopeProvider>());
        return services;
    }

    public static ClientRequestOptions ToClientRequestOptions(this HttpContext context) {
        return new ClientRequestOptions {
            Headers = new Dictionary<string, object> {
                { WebActorServiceScopeProvider.RequestServiceKey, context.RequestServices }
            }
        };
    }

    private sealed class WebActorServiceScopeProvider : IActorServiceScopeProvider {
        public const string RequestServiceKey = "X-Request-Services";

        private readonly SimpleActorServiceScopeProvider _simpleProvider;

        public WebActorServiceScopeProvider(IServiceScopeFactory serviceScopeFactory) {
            _simpleProvider = new SimpleActorServiceScopeProvider(serviceScopeFactory);
        }

        public IServiceScope GetActorScope(Envelope letter, IActorOptions? options = null) {
            if (letter.Headers.TryGetValue(RequestServiceKey, out object? scope) && scope is IServiceProvider serviceProvider) {
                // discard dispose functionality to prevent double dispose
                return new DelegateServiceScope(serviceProvider);
            }
            return _simpleProvider.GetActorScope(letter, options);
        }
    }
}
