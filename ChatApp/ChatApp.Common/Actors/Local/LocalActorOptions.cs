using ChatApp.Common.Actors.Abstractions;

namespace ChatApp.Common.Actors.Local;

public sealed record LocalActorOptions : IActorOptions {
    public ActorCallScope CallScope { get; init; } = ActorCallScope.PreserveScope;
    public int? MailboxCapacity { get; init; }
    public BackpressureBehaviour BackpressureBehaviour { get; init; } = BackpressureBehaviour.Fail;
}
