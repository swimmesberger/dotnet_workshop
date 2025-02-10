namespace ChatApp.Common.Actors.Abstractions;

public interface IActor {
    ValueTask OnLetter();
}
