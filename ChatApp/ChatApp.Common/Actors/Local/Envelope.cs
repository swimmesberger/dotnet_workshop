using System.Collections.ObjectModel;
using ChatApp.Common.Actors.Abstractions;

namespace ChatApp.Common.Actors.Local;

public sealed record Envelope : IEnvelope {
    public static readonly Envelope Unknown = new Envelope {
        Sender = IActorRef.Nobody,
        Body = IMessage.Unknown,
        CancellationToken = CancellationToken.None
    };

    public required ICanTell Sender { get; init; }
    public IReadOnlyDictionary<string, object> Headers { get; init; } = ReadOnlyDictionary<string, object>.Empty;
    public required IMessage Body { get; init; }
    public CancellationToken CancellationToken { get; init; }
    public List<IDisposable> Disposables { get; init; } = [];
}
