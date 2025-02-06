using ChatApp.Application.Domain.Users;
using ChatApp.Common.Actor.Abstractions;

namespace ChatApp.Application.Users;

public sealed class UserActor : IActor {
    private IActorContext Context { get; }
    private readonly UserService _userService;

    public UserActor(IActorContext context, UserService userService) {
        Context = context;
        _userService = userService;
    }

    public async ValueTask OnLetter(Envelope letter) {
        try {
            switch (letter.Body) {
                case CreateUserCommand createCommand:
                    var createdUser = await _userService.CreateUserAsync(createCommand.Username, letter.CancellationToken);
                    letter.Sender.Tell(new CreateUserCommand.Reply {
                        State = createdUser
                    }, Context.Self);
                    break;
                case GetAllUsersQuery:
                    var queriedUsers = await _userService.GetAllUsersAsync(letter.CancellationToken);
                    letter.Sender.Tell(new GetAllUsersQuery.Reply {
                        State = queriedUsers
                    }, Context.Self);
                    break;
                case GetUserByIdQuery getQuery:
                    var queriedUser = await _userService.GetUserByIdAsync(getQuery.Id, letter.CancellationToken);
                    letter.Sender.Tell(new GetUserByIdQuery.Reply {
                        State = queriedUser
                    }, Context.Self);
                    break;
                default:
                    letter.Sender.Tell(new FailureReply(new ArgumentException("Unhandled message")), Context.Self);
                    break;
                // Add more cases for other commands as needed
            }
        } catch (Exception ex) {
            letter.Sender.Tell(new FailureReply(ex), Context.Self);
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

