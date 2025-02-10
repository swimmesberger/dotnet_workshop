using System.Collections.ObjectModel;
using System.Threading.Channels;
using ChatApp.Common.Actors.Abstractions;
using ChatApp.Common.Actors.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatApp.Common.Actors.Local;

public sealed class LocalActorCell : IActorRef {
    public LocalActorContext Context { get; }

    private ILogger Logger { get; }
    private readonly IActor _actorInstance;

    private readonly Channel<Envelope> _messageChannel;
    private readonly TaskCompletionSource _stopped;

    public ActorConfiguration Configuration => Context.Configuration;
    public Type ActorType => Context.ActorType;
    private LocalActorOptions Options => (Configuration.Options! as LocalActorOptions)!;

    public LocalActorCell(ILogger logger, IActor actorInstance, LocalActorContext context) {
        Logger = logger;
        _actorInstance = actorInstance;
        Context = context;
        _messageChannel = Options.MailboxCapacity == null ? Channel.CreateUnbounded<Envelope>(new UnboundedChannelOptions {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        }) : Channel.CreateBounded<Envelope>(new BoundedChannelOptions(Options.MailboxCapacity.Value) {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
            FullMode = Options.BackpressureBehaviour switch {
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
        var letter = CreateEnvelope(message, options, askCts.Token);
        await WriteLetterAsync(letter);
        AfterWriteLetter(letter);
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
        var letter = CreateEnvelope(message, options, askCts.Token);
        await WriteLetterAsync(letter);
        AfterWriteLetter(letter);
        return await result.Task;
    }

    public void Tell(IMessage message, RequestOptions? options = null) {
        var letter = CreateEnvelope(message, options);
        WriteLetter(letter);
        AfterWriteLetter(letter);
    }

    private void WriteLetter(Envelope letter) {
        bool success = _messageChannel.Writer.TryWrite(letter);
        if (!success) {
            // can't happen when channel is unbounded
            letter.Sender.Tell(new FailureReply(new BackpressureException("Message channel is full")), this);
        }
    }

    private ValueTask WriteLetterAsync(Envelope letter) {
        if (Options.BackpressureBehaviour == BackpressureBehaviour.Fail) {
            if (!_messageChannel.Writer.TryWrite(letter)) {
                letter.Sender.Tell(new FailureReply(new BackpressureException("Message channel is full")), this);
            }
            return ValueTask.CompletedTask;
        } else {
            return _messageChannel.Writer.WriteAsync(letter, letter.CancellationToken);
        }
    }

    private void AfterWriteLetter(Envelope letter) {
        if (letter.CancellationToken.CanBeCanceled) {
            letter.Disposables.Add(letter.CancellationToken.Register(() => {
                letter.Sender.Tell(new FailureReply(new OperationCanceledException("Message was cancelled")), this);
            }));
        }
    }

    private async Task ProcessMessagesAsync() {
        try {
            await ProcessMessagesImplAsync();
        } finally {
            _stopped.TrySetResult();
        }
    }

    private async Task ProcessMessagesImplAsync() {
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
            try {
                await ProcessLetterAsync(letter, cancellationTokenSource.Token);
            } finally {
                await AfterLetterProcessed(letter);
            }
        }
    }

    private async ValueTask ProcessLetterAsync(Envelope letter, CancellationToken cancellationToken = default) {
        // skip message; replying is done in the token callback after writing to the channel
        if (letter.CancellationToken.IsCancellationRequested) {
            return;
        }

        CancellationTokenSource? messageCts = null;
        if (letter.CancellationToken.CanBeCanceled) {
            messageCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                letter.CancellationToken
            );
            Context.RequestAborted = messageCts.Token;
        } else {
            Context.RequestAborted = cancellationToken;
        }
        Context.Letter = letter;
        try {
            await _actorInstance.OnLetter();
        } catch (Exception e) {
            Logger.LogError(e, "Error processing message in actor {ActorType} {ActorId}", ActorType,
                Configuration.Id);
            Context.Letter.Sender.Tell(new FailureReply(e), this);
        } finally {
            messageCts?.Dispose();
        }
    }

    private async ValueTask AfterLetterProcessed(Envelope letter) {
        foreach(var disp in letter.Disposables) {
            disp.Dispose();
        }

        if (Context.RequestServiceScope != null) {
            await new AsyncServiceScope(Context.RequestServiceScope).DisposeAsync();
            Context.RequestServiceScope = null;
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
