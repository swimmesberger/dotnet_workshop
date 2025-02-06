using ChatApp.Actor.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Actor.Local;

public interface IActorServiceScopeProvider {
    IServiceScope GetActorScope(Envelope letter, ActorOptions options);
}

public static class ActorServiceScopeProviderExtensions {
    public static AsyncServiceScope GetActorAsyncScope(this IActorServiceScopeProvider provider, Envelope letter, ActorOptions options) {
        return new AsyncServiceScope(provider.GetActorScope(letter, options));
    }
}
