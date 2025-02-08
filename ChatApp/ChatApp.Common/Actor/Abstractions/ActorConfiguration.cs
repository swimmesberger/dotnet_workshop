namespace ChatApp.Common.Actor.Abstractions;

public sealed class ActorConfiguration<T> where T : IActor {
    public Type ActorType => typeof(T);
    public string? Id { get; init; }
    public IActorOptions? Options { get; init; }

    public ActorConfiguration ToBase() => new ActorConfiguration {
        ActorType = ActorType,
        Id = Id,
        Options = Options
    };
}

public sealed class ActorConfiguration {
    public required Type ActorType { get; init; }
    public string? Id { get; init; }
    public IActorOptions? Options { get; init; }
}
