namespace ChatApp.Actor.Abstractions;

public interface IActorContext {
    IActorRef Self { get; }
}
