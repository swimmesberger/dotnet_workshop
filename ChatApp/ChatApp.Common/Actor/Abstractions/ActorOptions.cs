namespace ChatApp.Common.Actor.Abstractions;

public sealed class ActorOptions {
    public ActorCallScope Scope { get; init; } = ActorCallScope.PreserveScope;
}
