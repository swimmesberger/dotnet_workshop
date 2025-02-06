namespace ChatApp.Common.Actors.Exceptions;

public abstract class ActorException : Exception {
    public ActorException() { }

    public ActorException(string? message) : base(message) { }

    public ActorException(string? message, Exception? innerException) : base(message, innerException) { }
}
