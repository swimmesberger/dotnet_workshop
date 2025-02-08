namespace ChatApp.Common.Actor.Abstractions;

public interface IActor {
    ValueTask OnLetter(Envelope letter);
}
