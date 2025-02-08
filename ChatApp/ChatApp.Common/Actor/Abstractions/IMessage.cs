using System.Runtime.ExceptionServices;

namespace ChatApp.Common.Actor.Abstractions;

public interface IMessage {
    public static readonly IMessage Unknown = new UnknownMessage();

    private sealed class UnknownMessage : IMessage;
}

public interface IReply<out T> : IMessage {
    T State { get; }
}

// ReSharper disable once UnusedTypeParameter
public interface IRequest<TReply> : IMessage;
// ReSharper disable once UnusedTypeParameter
public interface IRequest<T, TReply> : IRequest<T> where TReply: IReply<T>;
public interface IRequest : IMessage;

public sealed class PassivateCommand : IRequest {
    public static readonly PassivateCommand Instance = new PassivateCommand();

    private PassivateCommand() { }
}

public sealed class InitiateCommand : IRequest {
    public static readonly InitiateCommand Instance = new InitiateCommand();

    private InitiateCommand() { }
}

public sealed class GenericReply<T> : IReply<T> {
    public T State { get; init; }

    public GenericReply(T state) {
        State = state;
    }
}

public sealed class SuccessReply : IMessage {
    public static readonly SuccessReply Instance = new SuccessReply();

    public string? Message { get; init; }

    private SuccessReply(string? message = null) {
        Message = message;
    }
}

public sealed class FailureReply : IMessage {
    public ExceptionDispatchInfo? Exception { get; init; }

    public FailureReply(ExceptionDispatchInfo? exception = null) {
        Exception = exception;
    }

    public FailureReply(Exception? exception) : this(exception == null ? null : ExceptionDispatchInfo.Capture(exception)) { }
}
