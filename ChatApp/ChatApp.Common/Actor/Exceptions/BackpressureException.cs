namespace ChatApp.Common.Actor.Exceptions;

public sealed class BackpressureException : ActorException {
    public BackpressureException() { }

    public BackpressureException(string? message) : base(message) { }

    public BackpressureException(string? message, Exception? innerException) : base(message, innerException) { }
}
