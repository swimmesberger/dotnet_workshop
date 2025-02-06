namespace ChatApp.Common.Actor.Abstractions;

public interface ICanTell {
    void Tell(IMessage message, RequestOptions? options = null);
}

public interface IActorRef : ICanTell {
    public static readonly IActorRef Nobody = new NobodyInner();

    Task Ask(
        IRequest message,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default
    );

    Task<TResponse> Ask<TResponse>(
        IRequest<TResponse> message,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default
    );

    private sealed class NobodyInner : IActorRef {
        public void Tell(IMessage message, RequestOptions? options = null) {
            // Do nothing
        }

        public Task Ask(IRequest message, RequestOptions? options = null, CancellationToken cancellationToken = default) {
            return Task.CompletedTask;
        }

        public Task<TResponse> Ask<TResponse>(IRequest<TResponse> message, RequestOptions? options = null, CancellationToken cancellationToken = default) {
            return Task.FromResult<TResponse>(default!);
        }
    }
}

public sealed record RequestOptions {
    public ICanTell? Sender { get; init; }
    public TimeSpan? Timeout { get; init; }
    public IReadOnlyDictionary<string, object>? Headers { get; init; }
}
