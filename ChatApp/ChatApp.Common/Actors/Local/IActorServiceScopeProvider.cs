using ChatApp.Common.Actors.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Common.Actors.Local;

public interface IActorServiceScopeProvider {
    IServiceScope GetActorScope(IEnvelope letter, IActorOptions options);
}

public static class ActorServiceScopeProviderExtensions {
    public static AsyncServiceScope GetActorAsyncScope(this IActorServiceScopeProvider provider, IEnvelope letter, IActorOptions options) {
        return new AsyncServiceScope(provider.GetActorScope(letter, options));
    }
}

public sealed class EmptyActorServiceScopeProvider : IActorServiceScopeProvider {
    public static readonly EmptyActorServiceScopeProvider Instance = new();

    private EmptyActorServiceScopeProvider() { }

    public IServiceScope GetActorScope(IEnvelope letter, IActorOptions options) => EmptyServiceProvider.EmptyScope;
}
