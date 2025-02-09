using ChatApp.Common.Actors.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatApp.Common.Actors.Local;

public sealed class LocalActorSystem : IActorSystem {
    private static readonly int? DefaultMailboxCapacity = null;
    private static readonly LocalActorOptions DefaultOptions = new() {
        MailboxCapacity = DefaultMailboxCapacity,
        BackpressureBehaviour = BackpressureBehaviour.Fail
    };

    private ILogger Logger { get; }
    public LocalActorRegistry ActorRegistry { get; }
    private readonly IActorServiceScopeProvider _serviceScopeProvider;
    private readonly ActorSystemConfiguration _configuration;

    private LocalActorCell? _systemActor;

    public LocalActorSystem(
        ILogger<LocalActorSystem> logger,
        LocalActorRegistry actorRegistry,
        IActorServiceScopeProvider serviceScopeProvider,
        IOptions<ActorSystemConfiguration> actorConfiguration
    ) {
        Logger = logger;
        ActorRegistry = actorRegistry;
        _serviceScopeProvider = serviceScopeProvider;
        _configuration = actorConfiguration.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        Logger.LogInformation("Starting system actor");
        var systemActor = await CreateActorImplAsync(new ActorConfiguration {
            Id = "system",
            ActorType = typeof(SystemActor)
        }, cancellationToken);
        _systemActor = systemActor;
        Logger.LogInformation("Started system actor");
    }

    public async Task StopAsync(CancellationToken cancellationToken) {
        Logger.LogInformation("Stopping system actor");
        var systemActor = _systemActor;
        if (systemActor == null) {
            return;
        }
        await RemoveActorAsync(systemActor, cancellationToken);
        Logger.LogInformation("Stopped system actor");
    }

    public void Stop(IActorRef actorRef) {
        var systemActor = _systemActor;
        if (systemActor == null) {
            throw new InvalidOperationException("System actor is not initialized");
        }
        systemActor.Tell(new StopActorCommand {
            Actor = actorRef
        });
    }

    public async ValueTask StopAsync(IActorRef actorRef, CancellationToken cancellationToken = default) {
        var systemActor = _systemActor;
        if (systemActor == null) {
            throw new InvalidOperationException("System actor is not initialized");
        }
        await systemActor.Ask(new StopActorCommand {
            Actor = actorRef
        }, cancellationToken: cancellationToken);
    }

    public IActorRef? GetActor<T>(string? id = null) where T : IActor => GetActorImpl(typeof(T), id);

    public IActorRef? GetActor(ActorConfiguration configuration) => GetActorImpl(configuration);

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

    public ValueTask<IActorRef> GetOrCreateActorAsync<T>(ActorConfiguration<T> configuration, CancellationToken cancellationToken = default) where T : IActor {
        return GetOrCreateActorAsync(configuration.ToBase(), cancellationToken);
    }

    public async ValueTask<IActorRef> GetOrCreateActorAsync(ActorConfiguration configuration, CancellationToken cancellationToken = default) {
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

    public async ValueTask<IActorRef> CreateActorAsync<T>(ActorConfiguration<T> configuration, CancellationToken cancellationToken = default) where T : IActor {
        return await CreateActorAsync(configuration.ToBase(), cancellationToken);
    }

    public async ValueTask<IActorRef> CreateActorAsync(ActorConfiguration configuration, CancellationToken cancellationToken) {
        var systemActor = _systemActor;
        if (systemActor == null) {
            throw new InvalidOperationException("System actor is not initialized");
        }
        return await systemActor.Ask(new CreateActorCommand {
            ActorConfiguration = new ActorConfiguration {
                ActorType = configuration.ActorType,
                Id = configuration.Id,
                Options = configuration.Options
            }
        }, cancellationToken: cancellationToken);
    }

    internal async ValueTask StartImplAsync(CancellationToken cancellationToken = default) {
        // create all DI registered (root) actors
        foreach (var actorConfiguration in _configuration.RegisteredActors) {
            await CreateActorImplAsync(actorConfiguration, cancellationToken);
        }
    }

    internal async ValueTask StopImplAsync(CancellationToken cancellationToken = default) {
        List<LocalActorCell> actors = ActorRegistry.CopyAndClear();
        foreach (var actor in actors) {
            await StopActorAsync(actor, cancellationToken);
        }
    }

    internal async ValueTask RemoveActorAsync(IActorRef actorRef, CancellationToken cancellationToken = default) {
        if (!ActorRegistry.TryRemove(actorRef, out var actorCell)) {
            return;
        }
        await StopActorAsync(actorCell, cancellationToken);
    }

    private async ValueTask StopActorAsync(LocalActorCell actorCell, CancellationToken cancellationToken = default) {
        Logger.LogDebug("Stopping actor {ActorType} {ActorId}", actorCell.Configuration.ActorType, actorCell.Configuration.Id);
        await actorCell.Ask(PassivateCommand.Instance, cancellationToken: cancellationToken);
        await actorCell.StopAsync(cancellationToken);
        Logger.LogDebug("Stopped actor {ActorType} {ActorId}", actorCell.Configuration.ActorType, actorCell.Configuration.Id);
    }


    // sync access to the registry without actor access
    internal IActorRef? GetActorImpl(ActorConfiguration configuration) => ActorRegistry.GetActor(configuration.ActorType, configuration.Id);
    private IActorRef? GetActorImpl(Type actorType, string? id = null) => ActorRegistry.GetActor(actorType, id);

    internal async ValueTask<IActorRef> GetOrCreateActorImplAsync(ActorConfiguration configuration, CancellationToken cancellationToken = default) {
        return GetActorImpl(configuration) ?? await CreateActorAsync(configuration, cancellationToken);
    }

    internal async ValueTask<LocalActorCell> CreateActorImplAsync(ActorConfiguration configuration, CancellationToken cancellationToken = default) {
        Logger.LogDebug("Starting actor {ActorType} {ActorId}", configuration.ActorType, configuration.Id);
        var actorContext = new LocalActorContext {
            Configuration = configuration,
            ActorSystem = this
        };
        var actorFactory = new LocalActorInstanceFactory(actorContext);
        var actorProvider = await CreateActorProviderAsync(actorFactory);
        var cell = new LocalActorCell(actorProvider, configuration.Options as LocalActorOptions ?? DefaultOptions);
        actorContext.Self = cell;
        ActorRegistry.Register(cell);
        await cell.StartAsync(cancellationToken);
        await cell.Ask(InitiateCommand.Instance, cancellationToken: cancellationToken);
        Logger.LogDebug("Started actor {ActorType} {ActorId}", configuration.ActorType, configuration.Id);
        return cell;
    }

    private ValueTask<ILocalActorProvider> CreateActorProviderAsync(LocalActorInstanceFactory actorFactory) {
        var callScope = ActorCallScope.Singleton;
        if (actorFactory.Configuration.Options is LocalActorOptions localActorOptions) {
            callScope = localActorOptions.CallScope;
        }
        if (callScope == ActorCallScope.Singleton) {
            return SingletonActorProvider.CreateAsync(actorFactory, _serviceScopeProvider, actorFactory.Configuration.Options);
        }
        var result = callScope switch {
            ActorCallScope.PreserveScope => new ScopedActorProvider(actorFactory, _serviceScopeProvider),
            ActorCallScope.RequireScope => new ScopedActorProvider(actorFactory, _serviceScopeProvider),
            _ => throw new ArgumentOutOfRangeException()
        };
        return ValueTask.FromResult<ILocalActorProvider>(result);
    }
}

