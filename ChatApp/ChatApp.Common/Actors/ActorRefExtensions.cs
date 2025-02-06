using ChatApp.Common.Actors.Exceptions;

namespace ChatApp.Common.Actors;

public static class ActorRefExtensions {
    public static async Task Ask(
        this IActorRef actorRef,
        IRequest message,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    ) {
        timeout ??= Timeout.InfiniteTimeSpan;

        var result = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        CancellationTokenSource? timeoutCancellation = null;
        CancellationTokenRegistration? ctr1 = null;
        CancellationTokenRegistration? ctr2 = null;
        if (timeout != Timeout.InfiniteTimeSpan && timeout.Value > TimeSpan.Zero) {
            timeoutCancellation = new CancellationTokenSource();
            ctr1 = timeoutCancellation.Token.Register(() => {
                result.TrySetException(new AskTimeoutException($"Timeout after {timeout} seconds"));
            });

            timeoutCancellation.CancelAfter(timeout.Value);
        }
        if (cancellationToken.CanBeCanceled) {
            ctr2 = cancellationToken.Register(() => result.TrySetCanceled());
        }

        var promise = new PromiseActorRef(result);
        actorRef.Tell(message, promise);

        try {
            await result.Task;
        } finally {
            // ReSharper disable once MethodHasAsyncOverload
            ctr1?.Dispose();
            // ReSharper disable once MethodHasAsyncOverload
            ctr2?.Dispose();
            // ReSharper disable once MethodHasAsyncOverload
            timeoutCancellation?.Dispose();
        }
    }

    public static async Task<TResponse> Ask<TResponse>(
        this IActorRef actorRef,
        IRequest<TResponse> message,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    ) {
        timeout ??= Timeout.InfiniteTimeSpan;

        var result = new TaskCompletionSource<TResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

        CancellationTokenSource? timeoutCancellation = null;
        CancellationTokenRegistration? ctr1 = null;
        CancellationTokenRegistration? ctr2 = null;
        if (timeout != Timeout.InfiniteTimeSpan && timeout.Value > TimeSpan.Zero) {
            timeoutCancellation = new CancellationTokenSource();
            ctr1 = timeoutCancellation.Token.Register(() => {
                result.TrySetException(new AskTimeoutException($"Timeout after {timeout} seconds"));
            });

            timeoutCancellation.CancelAfter(timeout.Value);
        }
        if (cancellationToken.CanBeCanceled) {
            ctr2 = cancellationToken.Register(() => result.TrySetCanceled());
        }

        var promise = new PromiseActorRef<TResponse>(result);
        actorRef.Tell(message, promise);

        try {
            return await result.Task;
        } finally {
            // ReSharper disable once MethodHasAsyncOverload
            ctr1?.Dispose();
            // ReSharper disable once MethodHasAsyncOverload
            ctr2?.Dispose();
            // ReSharper disable once MethodHasAsyncOverload
            timeoutCancellation?.Dispose();
        }
    }

    private sealed class PromiseActorRef : IActorRef {
        private readonly TaskCompletionSource _result;

        public PromiseActorRef(TaskCompletionSource result) => _result = result;

        public void Tell(IMessage message, IActorRef? sender = null) {
            switch (message) {
                case SuccessReply:
                    _result.TrySetResult();
                    break;
                case FailureReply failureReply:
                    _result.TrySetException(failureReply.Exception?.SourceException ??
                                            new TaskCanceledException("Task cancelled by actor via Failure message."));
                    break;
                default:
                    _result.TrySetException(new ArgumentException(
                        $"Received message of type [{message.GetType()}] - Ask expected of type [{typeof(SuccessReply)}, {typeof(FailureReply)}]"));
                    break;
            }
        }
    }

    private sealed class PromiseActorRef<T> : IActorRef {
        private readonly TaskCompletionSource<T> _result;

        public PromiseActorRef(TaskCompletionSource<T> result) {
            _result = result;
        }

        public void Tell(IMessage message, IActorRef? sender = null) {
            switch (message) {
                case IReply<T> reply:
                    _result.TrySetResult(reply.State);
                    break;
                case FailureReply failureReply:
                    _result.TrySetException(failureReply.Exception?.SourceException ??
                                            new TaskCanceledException("Task cancelled by actor via Failure message."));
                    break;
                default:
                    _result.TrySetException(new ArgumentException(
                        $"Received message of type [{message.GetType()}] - Ask expected message of type [{typeof(T)}]"));
                    break;
            }
        }
    }
}
