using System.Threading.Channels;
using ChatApp.Actor.Abstractions;
using ChatApp.Actor.Exceptions;
using ChatApp.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ChatApp.Actor.Local;

public sealed class LocalActorSystem : IHostedService, IActorSystem {
    private static readonly int? DefaultMailboxCapacity = null;
    private static readonly LocalActorOptions DefaultOptions = new() {
        MailboxCapacity = DefaultMailboxCapacity,
        BackpressureBehaviour = BackpressureBehaviour.FailFast
    };

    private readonly ActorSystemConfiguration _configuration;
    private readonly IActorServiceScopeProvider _serviceScopeProvider;
    private readonly List<LocalActorCell> _actorCells;

    private AsyncServiceScope? _singletonStartScope;

    public LocalActorSystem(IOptions<ActorSystemConfiguration> actorConfiguration, IActorServiceScopeProvider serviceScopeProvider) {
        _configuration = actorConfiguration.Value;
        _serviceScopeProvider = serviceScopeProvider;
        _actorCells = new List<LocalActorCell>(_configuration.RegisteredActorTypes.Count);
    }

    public IActorRef? GetActor<T>() where T : IActor {
        return _actorCells.FirstOrDefault(x => x.ActorType == typeof(T));
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        foreach (var configuration in _configuration.RegisteredActors) {
            var actorContext = new LocalActorContext();
            var actorFactory = new ActorFactory(configuration, actorType => {
                if (actorType == typeof(IActorContext)) {
                    return actorContext;
                }
                return null;
            });
            var actorProvider = CreateActorProvider(actorFactory);
            var cell = new LocalActorCell(actorProvider, DefaultOptions);
            actorContext.Self = cell;
            _actorCells.Add(cell);
            await cell.StartAsync(cancellationToken);
        }

        if (_singletonStartScope != null) {
            await _singletonStartScope.Value.DisposeAsync();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken) {
        foreach (var cell in _actorCells) {
            await cell.StopAsync(cancellationToken);
        }
        _actorCells.Clear();
    }

    private IActorProvider CreateActorProvider(ActorFactory actorFactory) {
        if (actorFactory.Configuration.Options.Scope == ActorCallScope.Singleton) {
            _singletonStartScope ??= _serviceScopeProvider.GetActorAsyncScope(Envelope.Unknown, actorFactory.Configuration.Options);
            return SingletonActorProvider.Create(actorFactory, _singletonStartScope.Value.ServiceProvider);
        }
        return actorFactory.Configuration.Options.Scope switch {
            ActorCallScope.PreserveScope => new ScopedActorProvider(actorFactory, _serviceScopeProvider),
            ActorCallScope.RequireScope => new ScopedActorProvider(actorFactory, _serviceScopeProvider),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private sealed class LocalActorContext : IActorContext {
        public IActorRef Self { get; set; } = IActorRef.Nobody;
    }

    private sealed record LocalActorOptions {
        public int? MailboxCapacity { get; init; }
        public BackpressureBehaviour BackpressureBehaviour { get; init; } = BackpressureBehaviour.FailFast;
    }

    private interface IActorProvider {
        Type ActorType { get; }
        ValueTask<IActor> GetActorInstance(Envelope letter);
    }

    private sealed class ActorFactory {
        public Type ActorType => Configuration.ActorType;
        public ActorConfiguration Configuration { get; }
        private Func<Type, object?> DelegateServiceProvider { get; }

        public ActorFactory(ActorConfiguration configuration, Func<Type, object?> delegateServiceProvider) {
            Configuration = configuration;
            DelegateServiceProvider = delegateServiceProvider;
        }

        public IActor CreateActor(IServiceProvider? fallbackServiceProvider = null) {
            var actorContextServiceProvider = ServiceProviders.Create(DelegateServiceProvider, fallbackServiceProvider);
            if (ActivatorUtilities.CreateInstance(actorContextServiceProvider, ActorType) is not IActor actor) {
                throw new InvalidOperationException($"Failed to create actor of type {ActorType}");
            }
            return actor;
        }
    }

    private sealed class SingletonActorProvider : IActorProvider {
        public Type ActorType { get; }
        private readonly IActor _actor;

        private SingletonActorProvider(IActor actor, Type actorType) {
            _actor = actor;
            ActorType = actorType;
        }

        public ValueTask<IActor> GetActorInstance(Envelope letter) {
            return new ValueTask<IActor>(_actor);
        }

        public static SingletonActorProvider Create(ActorFactory factory, IServiceProvider serviceScopeProvider) {
            var actor = factory.CreateActor(serviceScopeProvider);
            return new SingletonActorProvider(actor, factory.ActorType);
        }
    }

    private sealed class ScopedActorProvider : IActorProvider {
        public Type ActorType => _actorFactory.ActorType;
        private readonly ActorFactory _actorFactory;
        private readonly IActorServiceScopeProvider _serviceScopeProvider;

        public ScopedActorProvider(ActorFactory actorFactory, IActorServiceScopeProvider serviceScopeProvider) {
            _actorFactory = actorFactory;
            _serviceScopeProvider = serviceScopeProvider;
        }

        public async ValueTask<IActor> GetActorInstance(Envelope letter) {
            await using var scope = _serviceScopeProvider.GetActorAsyncScope(letter, _actorFactory.Configuration.Options);
            return _actorFactory.CreateActor(scope.ServiceProvider);
        }
    }

    private sealed class LocalActorCell : IActorRef {
        public Type ActorType => _actorProvider.ActorType;

        private readonly IActorProvider _actorProvider;
        private readonly LocalActorOptions _options;
        private readonly Channel<Envelope> _messageChannel;
        private readonly TaskCompletionSource _stopped;

        public LocalActorCell(IActorProvider actorProvider, LocalActorOptions? options = null) {
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
                    BackpressureBehaviour.FailFast => BoundedChannelFullMode.Wait,
                    BackpressureBehaviour.Wait => BoundedChannelFullMode.Wait,
                    BackpressureBehaviour.DropNewest => BoundedChannelFullMode.DropNewest,
                    BackpressureBehaviour.DropOldest => BoundedChannelFullMode.DropOldest,
                    _ => throw new ArgumentOutOfRangeException()
                }
            });
            _stopped = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        }

        public Task StartAsync(CancellationToken cancellationToken) {
            _ = Task.Run(ProcessMessagesAsync, cancellationToken);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            _messageChannel.Writer.TryComplete();
            await _stopped.Task.WaitAsync(cancellationToken);
        }

        public async Task Ask(IRequest message, RequestOptions? options = null, CancellationToken cancellationToken = default) {
            var timeout = options?.Timeout;
            timeout ??= Timeout.InfiniteTimeSpan;

            var result = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var timeoutCancellation = new CancellationTokenSource();
            if (timeout != Timeout.InfiniteTimeSpan && timeout.Value > TimeSpan.Zero) {
                timeoutCancellation.CancelAfter(timeout.Value);
            }

            using var askCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellation.Token);
            var promise = new PromiseActorRef(result);
            var envelope = new Envelope {
                Sender = promise,
                Body = message,
                CancellationToken = cancellationToken,
                RequestId = options?.RequestId
            };
            await WriteLetterAsync(envelope, askCts.Token);
            await result.Task;
        }

        public async Task<TResponse> Ask<TResponse>(IRequest<TResponse> message, RequestOptions? options = null, CancellationToken cancellationToken = default) {
            var timeout = options?.Timeout;
            timeout ??= Timeout.InfiniteTimeSpan;

            var result = new TaskCompletionSource<TResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var timeoutCancellation = new CancellationTokenSource();
            if (timeout != Timeout.InfiniteTimeSpan && timeout.Value > TimeSpan.Zero) {
                timeoutCancellation.CancelAfter(timeout.Value);
            }

            using var askCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellation.Token);
            var promise = new PromiseActorRef<TResponse>(result);
            var envelope = new Envelope {
                Sender = promise,
                Body = message,
                CancellationToken = cancellationToken,
                RequestId = options?.RequestId
            };
            await WriteLetterAsync(envelope, askCts.Token);
            return await result.Task;
        }

        public void Tell(IMessage message, RequestOptions? options = null) {
            var sender = options?.Sender;
            sender ??= IActorRef.Nobody;
            var envelope = new Envelope {
                Sender = sender,
                Body = message,
                RequestId = options?.RequestId
            };
            WriteLetter(envelope);
        }

        private void WriteLetter(Envelope letter) {
            bool success = _messageChannel.Writer.TryWrite(letter);
            if (!success) {
                // can't happen when channel is unbounded
                letter.Sender.Tell(new FailureReply(new BackpressureException("Message channel is full")), this);
            }
        }

        private ValueTask WriteLetterAsync(Envelope letter, CancellationToken cancellationToken = default) {
            if (_options.BackpressureBehaviour == BackpressureBehaviour.FailFast) {
                if (!_messageChannel.Writer.TryWrite(letter)) {
                    letter.Sender.Tell(new FailureReply(new BackpressureException("Message channel is full")), this);
                }
                return ValueTask.CompletedTask;
            } else {
                return _messageChannel.Writer.WriteAsync(letter, cancellationToken);
            }
        }

        private async Task ProcessMessagesAsync() {
            try {
                // cts that is canceled when the channel is completed
                // this is used to cancel the currently executing message when the actor is stopped
                using var cancellationTokenSource = new CancellationTokenSource();
                _ =_messageChannel.Reader.Completion.ContinueWith(_ => {
                    // ReSharper disable once AccessToDisposedClosure
                    cancellationTokenSource.Cancel();
                }, TaskContinuationOptions.ExecuteSynchronously);
                await foreach (var envelope in _messageChannel.Reader.ReadAllAsync(CancellationToken.None)) {
                    using var messageCts = CancellationTokenSource.CreateLinkedTokenSource(
                        cancellationTokenSource.Token,
                        envelope.CancellationToken
                    );
                    var actor = await _actorProvider.GetActorInstance(envelope);
                    await actor.OnLetter(envelope, messageCts.Token);
                }
            } finally {
                _stopped.TrySetResult();
            }
        }
    }
}
