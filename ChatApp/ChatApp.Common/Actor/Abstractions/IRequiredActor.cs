namespace ChatApp.Common.Actor.Abstractions;

// ReSharper disable once UnusedTypeParameter
public interface IRequiredActor<T> where T: IActor {
    IActorRef ActorRef { get; }

    ValueTask<IActorRef> GetAsync(CancellationToken cancellationToken = default);
}

public sealed class RequiredActor<T> : IRequiredActor<T> where T: IActor {
    private readonly IActorSystem _actorSystem;
    public IActorRef ActorRef => _actorSystem.GetActor<T>() ?? throw new ArgumentException($"Can't find actor of type {typeof(T)}");

    public RequiredActor(IActorSystem actorSystem) {
        _actorSystem = actorSystem;
    }

    public async ValueTask<IActorRef> GetAsync(CancellationToken cancellationToken = default) =>
        await _actorSystem.GetActorAsync<T>(cancellationToken: cancellationToken) ?? throw new ArgumentException($"Can't find actor of type {typeof(T)}");
}
