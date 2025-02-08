using Microsoft.Extensions.Hosting;

namespace ChatApp.Common.Actor.Local;

public sealed class LocalActorService : IHostedService {
    private readonly LocalActorSystem _actorSystem;

    public LocalActorService(LocalActorSystem actorSystem) {
        _actorSystem = actorSystem;
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        await _actorSystem.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken) {
        await _actorSystem.StopAsync(cancellationToken);
    }
}
