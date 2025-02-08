namespace ChatApp.Common.Actor.Abstractions;

public interface IActorSystem {
    IActorRef? GetActor<T>(string? id = null) where T : IActor;

    ValueTask<IActorRef?> GetActorAsync<T>(string? id = null, CancellationToken cancellationToken = default)
        where T : IActor;

    ValueTask<IActorRef> GetOrCreateActorAsync<T>(ActorConfiguration<T> configuration,
        CancellationToken cancellationToken = default) where T : IActor;

    ValueTask<IActorRef> GetOrCreateActorAsync(ActorConfiguration configuration,
        CancellationToken cancellationToken = default);

    ValueTask<IActorRef> CreateActorAsync<T>(ActorConfiguration<T> configuration,
        CancellationToken cancellationToken = default) where T : IActor;

    ValueTask<IActorRef> CreateActorAsync(ActorConfiguration configuration,
        CancellationToken cancellationToken = default);

    void Stop(IActorRef actorRef);

    ValueTask StopAsync(IActorRef actorRef, CancellationToken cancellationToken = default);
}
