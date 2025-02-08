using System.Collections.ObjectModel;
using System.Threading.Channels;
using ChatApp.Common.Actor.Abstractions;
using ChatApp.Common.Actor.Exceptions;

namespace ChatApp.Common.Actor.Local;

public sealed class LocalActorCell : IActorRef {
    public Type ActorType => Context.ActorType;
    public LocalActorContext Context => _actorProvider.Context;
    public ActorConfiguration Configuration => Context.Configuration;

    private readonly ILocalActorProvider _actorProvider;
    private readonly LocalActorOptions _options;
    private readonly Channel<Envelope> _messageChannel;
    private readonly TaskCompletionSource _stopped;

    public LocalActorCell(ILocalActorProvider actorProvider, LocalActorOptions? options = null) {
        _actorProvider = actorProvider;
        _options = options ?? new LocalActorOptions();
        _messageChannel = _options.MailboxCapacity == null ? Channel.CreateUnbounded<Envelope>(new UnboundedChannelOptions {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        }) : Channel.CreateBounded<Envelope>(new BoundedChannelOptions(_options.MailboxCapacity.Value) {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
            FullMode = _options.BackpressureBehaviour switch {
                BackpressureBehaviour.Fail => BoundedChannelFullMode.Wait,
                BackpressureBehaviour.Wait => BoundedChannelFullMode.Wait,
                BackpressureBehaviour.DropNewest => BoundedChannelFullMode.DropNewest,
                BackpressureBehaviour.DropOldest => BoundedChannelFullMode.DropOldest,
                BackpressureBehaviour.DropWrite => BoundedChannelFullMode.DropWrite,
                _ => throw new ArgumentOutOfRangeException()
            }
        });
        _stopped = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

    }

    public void Start() {
        _ = Task.Run(ProcessMessagesAsync);
    }

    public void Stop() {
        _messageChannel.Writer.TryComplete();
    }

    public ValueTask StartAsync(CancellationToken cancellationToken = default) {
        _ = Task.Run(ProcessMessagesAsync, cancellationToken);
        return ValueTask.CompletedTask;
    }

    public async ValueTask StopAsync(CancellationToken cancellationToken = default) {
        _messageChannel.Writer.TryComplete();
        await _stopped.Task.WaitAsync(cancellationToken);
    }

    public async Task Ask(IRequest message, RequestOptions? options = null, CancellationToken cancellationToken = default) {
        options ??= new RequestOptions();
        if (options.Sender != null) {
            throw new ArgumentNullException(nameof(options.Sender));
        }
        if (options.Timeout == null) {
            options = options with { Timeout = Timeout.InfiniteTimeSpan };
        }

        var result = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var timeoutCancellation = new CancellationTokenSource();
        if (options.Timeout! != Timeout.InfiniteTimeSpan && options.Timeout!.Value > TimeSpan.Zero) {
            timeoutCancellation.CancelAfter(options.Timeout!.Value);
        }

        using var askCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellation.Token);
        options = options with { Sender = new PromiseActorRef(result) };
        var envelope = CreateEnvelope(message, options, askCts.Token);
        await WriteLetterAsync(envelope);
        await result.Task;
    }

    public async Task<TResponse> Ask<TResponse>(IRequest<TResponse> message, RequestOptions? options = null, CancellationToken cancellationToken = default) {
        options ??= new RequestOptions();
        if (options.Sender != null) {
            throw new ArgumentNullException(nameof(options.Sender));
        }
        if (options.Timeout == null) {
            options = options with { Timeout = Timeout.InfiniteTimeSpan };
        }

        var result = new TaskCompletionSource<TResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var timeoutCancellation = new CancellationTokenSource();
        if (options.Timeout! != Timeout.InfiniteTimeSpan && options.Timeout!.Value > TimeSpan.Zero) {
            timeoutCancellation.CancelAfter(options.Timeout!.Value);
        }

        using var askCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellation.Token);
        options = options with { Sender = new PromiseActorRef<TResponse>(result) };
        var envelope = CreateEnvelope(message, options, askCts.Token);
        await WriteLetterAsync(envelope);
        return await result.Task;
    }

    public void Tell(IMessage message, RequestOptions? options = null) {
        WriteLetter(CreateEnvelope(message, options));
    }

    private void WriteLetter(Envelope letter) {
        bool success = _messageChannel.Writer.TryWrite(letter);
        if (!success) {
            // can't happen when channel is unbounded
            letter.Sender.Tell(new FailureReply(new BackpressureException("Message channel is full")), this);
        }
    }

    private ValueTask WriteLetterAsync(Envelope letter) {
        if (_options.BackpressureBehaviour == BackpressureBehaviour.Fail) {
            if (!_messageChannel.Writer.TryWrite(letter)) {
                letter.Sender.Tell(new FailureReply(new BackpressureException("Message channel is full")), this);
            }
            return ValueTask.CompletedTask;
        } else {
            return _messageChannel.Writer.WriteAsync(letter, letter.CancellationToken);
        }
    }

    private async Task ProcessMessagesAsync() {
        try {
            // cts that is canceled when the channel is completed
            // this is used to cancel the currently executing message when the actor is stopped
            using var cancellationTokenSource = new CancellationTokenSource();
            _ = _messageChannel.Reader.Completion.ContinueWith(_ => {
                try {
                    // ReSharper disable once AccessToDisposedClosure
                    cancellationTokenSource.Cancel();
                } catch(ObjectDisposedException) { }
            }, TaskContinuationOptions.ExecuteSynchronously);
            await foreach (var letter in _messageChannel.Reader.ReadAllAsync(CancellationToken.None)) {
                using var messageCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationTokenSource.Token,
                    letter.CancellationToken
                );
                var localLetter = letter with {
                    CancellationToken = messageCts.Token
                };
                var actor = await _actorProvider.GetActorInstance(localLetter);
                await actor.OnLetter(localLetter);
            }
        } finally {
            _stopped.TrySetResult();
        }
    }

    private static Envelope CreateEnvelope(IMessage message, RequestOptions? options = null, CancellationToken cancellationToken = default) {
        var sender = options?.Sender;
        sender ??= IActorRef.Nobody;
        return new Envelope {
            Sender = sender,
            Body = message,
            Headers = options?.Headers ?? ReadOnlyDictionary<string, object>.Empty,
            CancellationToken = cancellationToken
        };
    }
}
