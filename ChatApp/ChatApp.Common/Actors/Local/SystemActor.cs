using ChatApp.Common.Actors.Abstractions;

namespace ChatApp.Common.Actors.Local;

public sealed class SystemActor : IActor {
    private LocalActorContext Context { get; }

    public SystemActor(LocalActorContext context) {
        Context = context;
    }

    public async ValueTask OnLetter() {
        try {
            switch (Context.Letter.Body) {
                case GetActorQuery getActorQuery:
                    Context.Letter.Sender.Tell(new GetActorQuery.Reply {
                        State = Context.ActorSystem.GetActorImpl(getActorQuery.ActorConfiguration)
                    });
                    break;
                case CreateActorCommand createActorCommand:
                    var createdActor = await Context.ActorSystem
                        .CreateActorImplAsync(createActorCommand.ActorConfiguration, Context.RequestAborted);
                    Context.Letter.Sender.Tell(new CreateActorCommand.Reply {
                        State = createdActor
                    });
                    break;
                case GetOrCreateActorCommand getOrCreateActorCommand:
                    var getOrCreatedActor = await Context.ActorSystem
                        .GetOrCreateActorImplAsync(getOrCreateActorCommand.ActorConfiguration, Context.RequestAborted);
                    Context.Letter.Sender.Tell(new GetOrCreateActorCommand.Reply {
                        State = getOrCreatedActor
                    });
                    break;
                case InitiateCommand:
                    await Context.ActorSystem.StartImplAsync(Context.RequestAborted);
                    Context.Letter.Sender.Tell(SuccessReply.Instance);
                    break;
                case PassivateCommand:
                    await Context.ActorSystem.StopImplAsync(Context.RequestAborted);
                    Context.Letter.Sender.Tell(SuccessReply.Instance);
                    break;
                case StopActorCommand stopActorCommand:
                    await Context.ActorSystem.RemoveActorAsync(stopActorCommand.Actor, Context.RequestAborted);
                    Context.Letter.Sender.Tell(SuccessReply.Instance);
                    break;
                default:
                    Context.Letter.Sender.Tell(new FailureReply(new ArgumentException("Unhandled message")));
                    break;
                // Add more cases for other commands as needed
            }
        } catch (Exception ex) {
            Context.Letter.Sender.Tell(new FailureReply(ex));
        }
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

public sealed record CreateActorCommand : IRequest<IActorRef, CreateActorCommand.Reply> {
    public required ActorConfiguration ActorConfiguration { get; init; }

    public sealed record Reply : IReply<IActorRef> {
        public required IActorRef State { get; init; }
    }
}

public sealed record StopActorCommand : IRequest {
    public required IActorRef Actor { get; init; }
}
