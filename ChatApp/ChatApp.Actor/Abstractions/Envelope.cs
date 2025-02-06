namespace ChatApp.Actor.Abstractions;

public sealed record Envelope {
    public static readonly Envelope Unknown = new Envelope {
        Sender = IActorRef.Nobody,
        Body = IMessage.Unknown,
        CancellationToken = CancellationToken.None
    };

    public required ICanTell Sender { get; init; }
    public required IMessage Body { get; init; }
    public string? RequestId { get; init; }
    public CancellationToken CancellationToken { get; init; }
}
