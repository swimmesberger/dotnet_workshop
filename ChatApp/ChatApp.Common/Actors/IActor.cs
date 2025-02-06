namespace ChatApp.Common.Actors;

public interface IActor {
    ValueTask OnLetter(Envelope envelope, CancellationToken cancellationToken = default);
}
