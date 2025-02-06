namespace ChatApp.Actor.Abstractions;

public interface IActor {
    ValueTask OnLetter(Envelope envelope, CancellationToken cancellationToken = default);
}
