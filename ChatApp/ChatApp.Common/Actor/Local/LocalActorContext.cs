using ChatApp.Common.Actor.Abstractions;

namespace ChatApp.Common.Actor.Local;

public sealed class LocalActorContext : IActorContext {
    public required ActorConfiguration Configuration { get; init; }
    public string? Id => Configuration.Id;
    public required LocalActorFactory ActorFactory { get; init; }
    IActorFactory IActorContext.ActorFactory => ActorFactory;
    public Type ActorType => Configuration.ActorType;
    public IActorRef Self { get; set; } = IActorRef.Nobody;
}
