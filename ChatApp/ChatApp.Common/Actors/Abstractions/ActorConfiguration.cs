namespace ChatApp.Common.Actors.Abstractions;

public sealed record ActorConfiguration<T> where T : IActor {
    public Type ActorType => typeof(T);
    public string? Id { get; init; }
    public IActorOptions? Options { get; init; }

    public ActorConfiguration ToBase() => new ActorConfiguration {
        ActorType = ActorType,
        Id = Id,
        Options = Options
    };
}

public sealed record ActorConfiguration {
    public required Type ActorType { get; init; }
    public string? Id { get; init; }
    public IActorOptions? Options { get; init; }
}
