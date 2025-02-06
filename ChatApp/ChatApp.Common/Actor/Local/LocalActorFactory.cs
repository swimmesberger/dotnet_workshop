using ChatApp.Common.Actor.Abstractions;

namespace ChatApp.Common.Actor.Local;

public sealed class LocalActorFactory : IActorFactory {
    private static readonly int? DefaultMailboxCapacity = null;
    private static readonly LocalActorOptions DefaultOptions = new() {
        MailboxCapacity = DefaultMailboxCapacity,
        BackpressureBehaviour = BackpressureBehaviour.FailFast
    };

    private readonly IActorServiceScopeProvider _serviceScopeProvider;

    public LocalActorFactory(IActorServiceScopeProvider serviceScopeProvider) => _serviceScopeProvider = serviceScopeProvider;

    async ValueTask<IActorRef> IActorFactory.CreateActorAsync(ActorConfiguration configuration, CancellationToken cancellationToken) {
        return await CreateActorAsync(configuration);
    }

    public async ValueTask<LocalActorCell> CreateActorAsync(ActorConfiguration configuration) {
        var actorContext = new LocalActorContext {
            Configuration = configuration,
            ActorFactory = this
        };
        var actorFactory = new LocalActorInstanceFactory(actorContext);
        var actorProvider = await CreateActorProviderAsync(actorFactory);
        var cell = new LocalActorCell(actorProvider, DefaultOptions);
        actorContext.Self = cell;
        return cell;
    }

    private ValueTask<ILocalActorProvider> CreateActorProviderAsync(LocalActorInstanceFactory actorFactory) {
        if (actorFactory.Configuration.Options.Scope == ActorCallScope.Singleton) {
            return SingletonActorProvider.CreateAsync(actorFactory, _serviceScopeProvider, actorFactory.Configuration.Options);
        }
        var result = actorFactory.Configuration.Options.Scope switch {
            ActorCallScope.PreserveScope => new ScopedActorProvider(actorFactory, _serviceScopeProvider),
            ActorCallScope.RequireScope => new ScopedActorProvider(actorFactory, _serviceScopeProvider),
            _ => throw new ArgumentOutOfRangeException()
        };
        return ValueTask.FromResult<ILocalActorProvider>(result);
    }
}
