using ChatApp.Common.Actor.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Common.Actor.Local;

public sealed class LocalActorInstanceFactory {
    public Type ActorType => Context.ActorType;
    public ActorConfiguration Configuration => Context.Configuration;
    public LocalActorContext Context { get; }
    private Func<Type, object?> DelegateServiceProvider { get; }

    public LocalActorInstanceFactory(LocalActorContext context) {
        Context = context;
        DelegateServiceProvider = actorType => {
            if (actorType == typeof(IActorContext)) {
                return context;
            }
            if (actorType == typeof(LocalActorContext)) {
                return context;
            }
            return null;
        };
    }

    public IActor CreateActor(IServiceProvider? fallbackServiceProvider = null) {
        var actorContextServiceProvider = ServiceProviders.Create(DelegateServiceProvider, fallbackServiceProvider);
        if (ActivatorUtilities.CreateInstance(actorContextServiceProvider, ActorType) is not IActor actor) {
            throw new InvalidOperationException($"Failed to create actor of type {ActorType}");
        }
        return actor;
    }
}
