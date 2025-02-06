using System.Threading.Channels;
using ChatApp.Common.Actors.Exceptions;
using Microsoft.Extensions.Hosting;

namespace ChatApp.Common.Actors;

public sealed class ActorSystem : IHostedService {
    private readonly List<IActor> _actors;
    private readonly List<ActorCell> _actorCells;

    public ActorSystem(IEnumerable<IActor> actors) {
        _actors = actors.ToList();
        _actorCells = new List<ActorCell>(_actors.Count);
    }

    public IActorRef? GetActor<T>() where T : IActor {
        return _actorCells.FirstOrDefault(x => x.ActorType == typeof(T));
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        foreach (var actor in _actors) {
            var cell = new ActorCell(actor);
            _actorCells.Add(cell);
            await cell.StartAsync(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken) {
        foreach (var cell in _actorCells) {
            await cell.StopAsync(cancellationToken);
        }
        _actors.Clear();
        _actorCells.Clear();
    }

    private sealed class ActorCell : IActorRef {
        public Type ActorType => _actor.GetType();

        private readonly IActor _actor;
        private readonly Channel<Envelope> _messageChannel = Channel.CreateUnbounded<Envelope>();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly TaskCompletionSource _stopped =
            new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        public ActorCell(IActor actor) => _actor = actor;

        public Task StartAsync(CancellationToken cancellationToken) {
            _ = Task.Run(ProcessMessagesAsync, cancellationToken);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            await _cancellationTokenSource.CancelAsync();
            await _stopped.Task;
        }


        public void Tell(IMessage message, IActorRef? sender = null) {
            sender ??= IActorRef.Nobody;
            var envelope = new Envelope() {
                Sender = sender,
                Body = message
            };
            bool success = _messageChannel.Writer.TryWrite(envelope);
            if (!success) {
                // can't happen because channel is unbounded
                throw new BackpressureException("Message channel is full");
            }
        }

        private async Task ProcessMessagesAsync() {
            try {
                await foreach (var envelope in _messageChannel.Reader.ReadAllAsync(_cancellationTokenSource.Token)) {
                    await _actor.OnLetter(envelope, _cancellationTokenSource.Token);
                }
            } finally {
                _stopped.TrySetResult();
            }
        }

    }
}
