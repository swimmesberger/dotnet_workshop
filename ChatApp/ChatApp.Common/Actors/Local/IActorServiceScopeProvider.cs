using ChatApp.Common.Actors.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Common.Actors.Local;

public interface IActorServiceScopeProvider {
    IServiceScope GetActorScope(Envelope letter, IActorOptions? options = null);
}

public static class ActorServiceScopeProviderExtensions {
    public static AsyncServiceScope GetActorAsyncScope(this IActorServiceScopeProvider provider, Envelope letter, IActorOptions? options = null) {
        return new AsyncServiceScope(provider.GetActorScope(letter, options));
    }
}
