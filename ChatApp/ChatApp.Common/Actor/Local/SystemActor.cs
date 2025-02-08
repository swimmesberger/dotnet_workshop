using ChatApp.Common.Actor.Abstractions;
using Microsoft.Extensions.Options;

namespace ChatApp.Common.Actor.Local;

public sealed class SystemActor : IActor {
    private LocalActorContext Context { get; }
    private readonly ActorSystemConfiguration _configuration;
    private LocalActorRegistry ActorRegistry => Context.ActorRegistry;

    public SystemActor(LocalActorContext context, IOptions<ActorSystemConfiguration> actorConfiguration) {
        Context = context;
        _configuration = actorConfiguration.Value;
    }

    public async ValueTask OnLetter(Envelope letter) {
        try {
            switch (letter.Body) {
                case GetActorQuery getActorQuery:
                    letter.Sender.Tell(new GetActorQuery.Reply {
                        State = GetActor(getActorQuery.ActorConfiguration)
                    });
                    break;
                case GetOrCreateActorCommand getOrCreateActorCommand:
                    letter.Sender.Tell(new GetOrCreateActorCommand.Reply {
                        State = await GetOrCreateActorAsync(getOrCreateActorCommand.ActorConfiguration, letter.CancellationToken)
                    });
                    break;
                case InitiateCommand:
                    // create all DI registered (root) actors
                    foreach (var actorConfiguration in _configuration.RegisteredActors) {
                        await CreateActorAsync(actorConfiguration, letter.CancellationToken);
                    }
                    letter.Sender.Tell(SuccessReply.Instance);
                    break;
                case PassivateCommand:
                    List<LocalActorCell> actors = ActorRegistry.CopyAndClear();
                    foreach (var actor in actors) {
                        await actor.StopAsync(letter.CancellationToken);
                    }
                    letter.Sender.Tell(SuccessReply.Instance);
                    break;
                default:
                    letter.Sender.Tell(new FailureReply(new ArgumentException("Unhandled message")));
                    break;
                // Add more cases for other commands as needed
            }
        } catch (Exception ex) {
            letter.Sender.Tell(new FailureReply(ex));
        }
    }

    private ValueTask<IActorRef> GetOrCreateActorAsync(ActorConfiguration configuration, CancellationToken cancellationToken = default) {
        var actor = GetActor(configuration);
        return actor == null ? CreateActorAsync(configuration, cancellationToken) : ValueTask.FromResult(actor);
    }

    private IActorRef? GetActor(ActorConfiguration configuration) {
        return ActorRegistry.GetActor(configuration.ActorType, configuration.Id);
    }

    private async ValueTask<IActorRef> CreateActorAsync(ActorConfiguration configuration, CancellationToken cancellationToken = default) {
        var cell = await Context.ActorFactory.CreateActorAsync(configuration);
        ActorRegistry.Register(cell);
        await cell.StartAsync(cancellationToken);
        return cell;
    }
}

public sealed record GetActorQuery : IRequest<IActorRef?, GetActorQuery.Reply> {
    public required ActorConfiguration ActorConfiguration { get; init; }

    public sealed record Reply : IReply<IActorRef?> {
        public IActorRef? State { get; init; }
    }
}

public sealed record GetOrCreateActorCommand : IRequest<IActorRef, GetOrCreateActorCommand.Reply> {
    public required ActorConfiguration ActorConfiguration { get; init; }

    public sealed record Reply : IReply<IActorRef> {
        public required IActorRef State { get; init; }
    }
}
