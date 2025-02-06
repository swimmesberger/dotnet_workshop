namespace ChatApp.Common.Actor.Abstractions;

public interface IActorContext {
    string? Id { get; }

    Type ActorType { get; }

    IActorFactory ActorFactory { get; }

    IActorRef Self { get; }
}
