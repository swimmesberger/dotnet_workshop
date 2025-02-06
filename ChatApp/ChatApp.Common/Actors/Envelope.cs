namespace ChatApp.Common.Actors;

public sealed record Envelope {
    public required IActorRef Sender { get; init; }
    public required IMessage Body { get; init; }
}
