namespace ChatApp.Common.Actor.Abstractions;

public interface IActorFactory {
    ValueTask<IActorRef> CreateActorAsync(ActorConfiguration configuration, CancellationToken cancellationToken = default);
}
