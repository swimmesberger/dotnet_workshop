namespace ChatApp.Actor.Abstractions;

internal sealed class PromiseActorRef<T> : ICanTell {
    private readonly TaskCompletionSource<T> _result;

    public PromiseActorRef(TaskCompletionSource<T> result) {
        _result = result;
    }

    public void Tell(IMessage message, RequestOptions? options = null) {
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
