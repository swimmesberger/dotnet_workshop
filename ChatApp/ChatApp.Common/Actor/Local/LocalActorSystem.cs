using ChatApp.Common.Actor.Abstractions;
using Microsoft.Extensions.Hosting;

namespace ChatApp.Common.Actor.Local;

public sealed class LocalActorSystem : IHostedService, IActorSystem {
    private readonly LocalActorFactory _actorFactory;
    private readonly LocalActorRegistry _actorRegistry;
    private LocalActorCell? _systemActor;

    public LocalActorSystem(LocalActorFactory actorFactory, LocalActorRegistry actorRegistry) {
        _actorFactory = actorFactory;
        _actorRegistry = actorRegistry;
    }

    public IActorRef? GetActor<T>(string? id = null) where T : IActor {
        // sync access to the registry without actor access
        return _actorRegistry.GetActor(typeof(T), id);
    }

    public async ValueTask<IActorRef?> GetActorAsync<T>(string? id = null, CancellationToken cancellationToken = default) where T : IActor {
        var systemActor = _systemActor;
        if (systemActor == null) {
            return null;
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
            return IActorRef.Nobody;
        }
        return await systemActor.Ask(new GetOrCreateActorCommand {
            ActorConfiguration = new ActorConfiguration {
                ActorType = configuration.ActorType,
                Id = configuration.Id,
                Options = configuration.Options
            }
        }, cancellationToken: cancellationToken);
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        var systemActor = await _actorFactory.CreateActorAsync(new ActorConfiguration {
            Id = "system",
            ActorType = typeof(SystemActor)
        });
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
}
