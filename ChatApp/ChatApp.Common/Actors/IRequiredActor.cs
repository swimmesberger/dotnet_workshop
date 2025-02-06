namespace ChatApp.Common.Actors;

// ReSharper disable once UnusedTypeParameter
public interface IRequiredActor<T> where T: IActor {
    IActorRef ActorRef { get; }
}

public sealed class RequiredActor<T> : IRequiredActor<T> where T: IActor {
    private readonly ActorSystem _actorSystem;
    public IActorRef ActorRef => _actorSystem.GetActor<T>() ?? throw new ArgumentException($"Can't find actor of type {typeof(T)}");

    public RequiredActor(ActorSystem actorSystem) {
        _actorSystem = actorSystem;
    }
}
