namespace ChatApp.Common.Actors.Abstractions;

public interface IEnvelope {
    ICanTell Sender { get; }
    IReadOnlyDictionary<string, object> Headers { get; }
    IMessage Body { get; }
}
