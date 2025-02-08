using ChatApp.Common.Actor.Abstractions;

namespace ChatApp.Common.Actor.Local;

public sealed class ActorSystemConfiguration {
    private readonly List<ActorConfiguration> _registeredActors = [];
    public IReadOnlyList<ActorConfiguration> RegisteredActors => _registeredActors;
    public IReadOnlyList<Type> RegisteredActorTypes => RegisteredActors.Select(x => x.ActorType).ToList();

    public ActorSystemConfiguration RegisterActor<TActor>(LocalActorOptions? options = null) where TActor : IActor {
        RegisterActor(typeof(TActor), options);
        return this;
    }

    public ActorSystemConfiguration RegisterActor(Type actorType, LocalActorOptions? options = null) {
        _registeredActors.Add(new ActorConfiguration {
            ActorType = actorType,
            Options = options ?? new LocalActorOptions(),
        });
        return this;
    }

    public ActorConfiguration? GetActorConfiguration<T>() where T : IActor {
        return GetActorConfiguration(typeof(T));
    }

    public ActorConfiguration? GetActorConfiguration(Type actorType) {
        return RegisteredActors.FirstOrDefault(x => x.ActorType == actorType);
    }
}
