namespace ChatApp.Common.Actor.Exceptions;

public sealed class AskTimeoutException : ActorException {
    public AskTimeoutException() { }

    public AskTimeoutException(string? message) : base(message) { }

    public AskTimeoutException(string? message, Exception? innerException) : base(message, innerException) { }
}
