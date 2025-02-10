using ChatApp.Application.Domain.Users;
using ChatApp.Common.Actors.Abstractions;
using ChatApp.Common.Actors.Local;

namespace ChatApp.Application.Users;

public sealed class UserManagementActor : IActor {
    private IActorContext Context { get; }
    private readonly UserManagementService _userService;

    public UserManagementActor(IActorContext context, UserManagementService userService) {
        Context = context;
        _userService = userService;
    }

    public async ValueTask OnLetter() {
        try {
            switch (Context.Letter.Body) {
                case InitiateCommand or PassivateCommand:
                    Context.Letter.Sender.Tell(SuccessReply.Instance);
                    break;
                case CreateUserCommand createCommand:
                    var createdUser = await _userService.CreateUserAsync(createCommand.Username, Context.RequestAborted);
                    Context.Letter.Sender.Tell(new CreateUserCommand.Reply {
                        State = createdUser
                    }, Context.Self);
                    break;
                case GetAllUsersQuery:
                    var queriedUsers = await _userService.GetAllUsersAsync(Context.RequestAborted);
                    Context.Letter.Sender.Tell(new GetAllUsersQuery.Reply {
                        State = queriedUsers
                    }, Context.Self);
                    break;
                case GetUserByIdQuery getQuery:
                    var queriedUser = await _userService.GetUserByIdAsync(getQuery.Id, Context.RequestAborted);
                    Context.Letter.Sender.Tell(new GetUserByIdQuery.Reply {
                        State = queriedUser
                    }, Context.Self);
                    break;
                default:
                    Context.Letter.Sender.Tell(new FailureReply(new ArgumentException("Unhandled message")), Context.Self);
                    break;
                // Add more cases for other commands as needed
            }
        } catch (Exception ex) {
            Context.Letter.Sender.Tell(new FailureReply(ex), Context.Self);
        }
    }
}

public sealed record CreateUserCommand : IRequest<User, CreateUserCommand.Reply> {
    public required string Username { get; init; }

    public sealed class Reply : IReply<User> {
        public required User State { get; init; }
    }
}

public sealed record GetAllUsersQuery : IRequest<List<User>, GetAllUsersQuery.Reply> {
    public sealed class Reply : IReply<List<User>> {
        public required List<User> State { get; init; }
    }
}

public sealed record GetUserByIdQuery : IRequest<User?, GetUserByIdQuery.Reply> {
    public required int Id { get; init; }

    public sealed class Reply : IReply<User?> {
        public required User? State { get; init; }
    }
}

