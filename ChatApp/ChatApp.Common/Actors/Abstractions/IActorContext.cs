namespace ChatApp.Common.Actors.Abstractions;

public interface IActorContext {
    string? Id { get; }

    Type ActorType { get; }

    IActorSystem ActorSystem { get; }

    IActorRef Self { get; }
}
