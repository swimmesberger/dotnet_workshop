using ChatApp.Common.Actor.Abstractions;

namespace ChatApp.Common.Actor.Local;

public sealed class LocalActorContext : IActorContext {
    public required LocalActorSystem ActorSystem { get; init; }
    IActorSystem IActorContext.ActorSystem => ActorSystem;
    public LocalActorSystem ActorFactory => ActorSystem;
    public LocalActorRegistry ActorRegistry => ActorSystem.ActorRegistry;

    public required ActorConfiguration Configuration { get; init; }
    public string? Id => Configuration.Id;
    public Type ActorType => Configuration.ActorType;
    public IActorRef Self { get; set; } = IActorRef.Nobody;

    public ValueTask<IActorRef> GetOrCreateActorAsync<T>(ActorConfiguration<T> configuration, CancellationToken cancellationToken = default) where T : IActor => ActorSystem.GetOrCreateActorAsync(configuration, cancellationToken);

    public IActorRef? GetActor<T>(string? id = null) where T : IActor => ActorSystem.GetActor<T>(id);

    public ValueTask<IActorRef?> GetActorAsync<T>(string? id = null, CancellationToken cancellationToken = default) where T : IActor => ActorSystem.GetActorAsync<T>(id, cancellationToken);
}
