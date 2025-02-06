using ChatApp.Actor.Abstractions;

namespace ChatApp.Actor.Local;

public sealed class ActorSystemConfiguration {
    private readonly List<ActorConfiguration> _registeredActors = [];
    public IReadOnlyList<ActorConfiguration> RegisteredActors => _registeredActors;
    public IReadOnlyList<Type> RegisteredActorTypes => RegisteredActors.Select(x => x.ActorType).ToList();

    public ActorSystemConfiguration RegisterActor<TActor>(ActorOptions? options = null) where TActor : IActor {
        RegisterActor(typeof(TActor), options);
        return this;
    }

    public ActorSystemConfiguration RegisterActor(Type actorType, ActorOptions? options = null) {
        _registeredActors.Add(new ActorConfiguration {
            ActorType = actorType,
            Options = options ?? new ActorOptions(),
        });
        return this;
    }
}

public sealed class ActorOptions {
    public ActorCallScope Scope { get; init; } = ActorCallScope.PreserveScope;
}

public sealed class ActorConfiguration {
    public required Type ActorType { get; init; }
    public ActorOptions Options { get; init; } = new ActorOptions();
}

public enum ActorCallScope {
    None,
    Singleton,
    PreserveScope,
    RequireScope,
}
