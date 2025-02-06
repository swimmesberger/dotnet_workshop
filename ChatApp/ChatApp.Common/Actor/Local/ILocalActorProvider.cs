using ChatApp.Common.Actor.Abstractions;

namespace ChatApp.Common.Actor.Local;

public interface ILocalActorProvider {
    LocalActorContext Context { get; }
    ValueTask<IActor> GetActorInstance(Envelope letter);
}


internal sealed class SingletonActorProvider : ILocalActorProvider {
    public LocalActorContext Context { get; }
    private readonly IActor _actor;

    private SingletonActorProvider(IActor actor, LocalActorContext context) {
        _actor = actor;
        Context = context;
    }

    public ValueTask<IActor> GetActorInstance(Envelope letter) {
        return new ValueTask<IActor>(_actor);
    }

    public static async ValueTask<ILocalActorProvider> CreateAsync(LocalActorInstanceFactory factory, IActorServiceScopeProvider serviceScopeProvider, ActorOptions options) {
        await using var scope = serviceScopeProvider.GetActorAsyncScope(Envelope.Unknown, options);
        var actor = factory.CreateActor(scope.ServiceProvider);
        return new SingletonActorProvider(actor, factory.Context);
    }
}

internal sealed class ScopedActorProvider : ILocalActorProvider {
    public LocalActorContext Context => _actorFactory.Context;
    private readonly LocalActorInstanceFactory _actorFactory;
    private readonly IActorServiceScopeProvider _serviceScopeProvider;

    public ScopedActorProvider(LocalActorInstanceFactory actorFactory, IActorServiceScopeProvider serviceScopeProvider) {
        _actorFactory = actorFactory;
        _serviceScopeProvider = serviceScopeProvider;
    }

    public async ValueTask<IActor> GetActorInstance(Envelope letter) {
        await using var scope = _serviceScopeProvider.GetActorAsyncScope(letter, _actorFactory.Configuration.Options);
        return _actorFactory.CreateActor(scope.ServiceProvider);
    }
}
