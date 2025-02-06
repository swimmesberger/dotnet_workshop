namespace ChatApp.Common.Actor.Abstractions;

public interface IActorSystem {
    IActorRef? GetActor<T>(string? id = null) where T : IActor;

    ValueTask<IActorRef?> GetActorAsync<T>(string? id = null, CancellationToken cancellationToken = default) where T : IActor;

    ValueTask<IActorRef> GetOrCreateActorAsync<T>(ActorConfiguration<T> configuration, CancellationToken cancellationToken = default) where T : IActor;

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
