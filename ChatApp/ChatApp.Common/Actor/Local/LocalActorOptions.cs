using ChatApp.Common.Actor.Abstractions;

namespace ChatApp.Common.Actor.Local;

public sealed record LocalActorOptions : IActorOptions {
    public ActorCallScope CallScope { get; init; } = ActorCallScope.PreserveScope;
    public int? MailboxCapacity { get; init; }
    public BackpressureBehaviour BackpressureBehaviour { get; init; } = BackpressureBehaviour.Fail;
}
