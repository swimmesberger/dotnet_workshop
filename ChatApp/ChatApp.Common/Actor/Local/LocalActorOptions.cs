namespace ChatApp.Common.Actor.Local;

public sealed record LocalActorOptions {
    public int? MailboxCapacity { get; init; }
    public BackpressureBehaviour BackpressureBehaviour { get; init; } = BackpressureBehaviour.Fail;
}
