namespace ChatApp.Common.Actor.Abstractions;

public interface IActorContext {
    string? Id { get; }

    Type ActorType { get; }

    IActorSystem ActorSystem { get; }

    IActorRef Self { get; }
}
