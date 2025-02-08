using ChatApp.Common.Actor.Abstractions;

namespace ChatApp.Common.Actor.Local;

public sealed class LocalActorSystem : IActorSystem {
    private static readonly int? DefaultMailboxCapacity = null;
    private static readonly LocalActorOptions DefaultOptions = new() {
        MailboxCapacity = DefaultMailboxCapacity,
        BackpressureBehaviour = BackpressureBehaviour.Fail
    };

    public LocalActorRegistry ActorRegistry { get; }
    private readonly IActorServiceScopeProvider _serviceScopeProvider;

    private LocalActorCell? _systemActor;

    public LocalActorSystem(LocalActorRegistry actorRegistry, IActorServiceScopeProvider serviceScopeProvider) {
        ActorRegistry = actorRegistry;
        _serviceScopeProvider = serviceScopeProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        var systemActor = await CreateActorAsync(new ActorConfiguration {
            Id = "system",
            ActorType = typeof(SystemActor)
        });
        ActorRegistry.Register(systemActor);
        await systemActor.StartAsync(cancellationToken);
        await systemActor.Ask(InitiateCommand.Instance, cancellationToken: cancellationToken);
        _systemActor = systemActor;
    }

    public async Task StopAsync(CancellationToken cancellationToken) {
        var systemActor = _systemActor;
        if (systemActor == null) {
            return;
        }
        await systemActor.Ask(PassivateCommand.Instance, cancellationToken: cancellationToken);
    }

    // sync access to the registry without actor access
    public IActorRef? GetActor<T>(string? id = null) where T : IActor => ActorRegistry.GetActor<T>(id);

    public async ValueTask<IActorRef?> GetActorAsync<T>(string? id = null, CancellationToken cancellationToken = default) where T : IActor {
        var systemActor = _systemActor;
        if (systemActor == null) {
            throw new InvalidOperationException("System actor is not initialized");
        }
        return await systemActor.Ask(new GetActorQuery {
            ActorConfiguration = new ActorConfiguration {
                ActorType = typeof(T),
                Id = id
            }
        }, cancellationToken: cancellationToken);
    }

    public async ValueTask<IActorRef> GetOrCreateActorAsync<T>(ActorConfiguration<T> configuration, CancellationToken cancellationToken = default) where T : IActor {
        var systemActor = _systemActor;
        if (systemActor == null) {
            throw new InvalidOperationException("System actor is not initialized");
        }
        return await systemActor.Ask(new GetOrCreateActorCommand {
            ActorConfiguration = new ActorConfiguration {
                ActorType = configuration.ActorType,
                Id = configuration.Id,
                Options = configuration.Options
            }
        }, cancellationToken: cancellationToken);
    }

    async ValueTask<IActorRef> IActorSystem.CreateActorAsync(ActorConfiguration configuration, CancellationToken cancellationToken) {
        return await CreateActorAsync(configuration);
    }

    public async ValueTask<LocalActorCell> CreateActorAsync(ActorConfiguration configuration) {
        var actorContext = new LocalActorContext {
            Configuration = configuration,
            ActorSystem = this
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

