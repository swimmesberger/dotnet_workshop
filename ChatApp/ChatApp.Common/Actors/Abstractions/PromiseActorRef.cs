namespace ChatApp.Common.Actors.Abstractions;

internal sealed class PromiseActorRef : ICanTell {
    private readonly TaskCompletionSource _result;

    public PromiseActorRef(TaskCompletionSource result) => _result = result;

    public void Tell(IMessage message, RequestOptions? options = null) {
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
