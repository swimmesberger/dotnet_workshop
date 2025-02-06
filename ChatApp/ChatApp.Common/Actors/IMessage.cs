using System.Runtime.ExceptionServices;

namespace ChatApp.Common.Actors;

public interface IMessage;

public interface IReply<out T> : IMessage {
    T State { get; }
}

// ReSharper disable once UnusedTypeParameter
public interface IRequest<TReply> : IMessage;
// ReSharper disable once UnusedTypeParameter
public interface IRequest<T, TReply> : IRequest<T> where TReply: IReply<T>;
public interface IRequest : IMessage;

public sealed class GenericReply<T> : IReply<T> {
    public T State { get; init; }

    public GenericReply(T state) {
        State = state;
    }
}

public sealed class SuccessReply : IMessage {
    public static readonly SuccessReply Instance = new SuccessReply();

    public string? Message { get; init; }

    public SuccessReply(string? message = null) {
        Message = message;
    }
}

public sealed class FailureReply : IMessage {
    public ExceptionDispatchInfo? Exception { get; init; }

    public FailureReply(ExceptionDispatchInfo? exception = null) {
        Exception = exception;
    }
}
