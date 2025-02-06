using System.Runtime.ExceptionServices;
using ChatApp.Common.Actors;
using ChatApp.Domain.Users;

namespace ChatApp.Application.Users;

public sealed class UserActor : IActor {
    private readonly UserService _userService;

    public UserActor(UserService userService) {
        _userService = userService;
    }

    public async ValueTask OnLetter(Envelope envelope, CancellationToken cancellationToken = default) {
        try {
            switch (envelope.Body) {
                case CreateUserCommand createCommand:
                    var createdUser = await _userService.CreateUserAsync(createCommand.Username, cancellationToken);
                    envelope.Sender.Tell(new CreateUserCommand.Reply {
                        State = createdUser
                    });
                    break;
                case GetAllUsersQuery:
                    var queriedUsers = await _userService.GetAllUsersAsync(cancellationToken);
                    envelope.Sender.Tell(new GetAllUsersQuery.Reply {
                        State = queriedUsers
                    });
                    break;
                case GetUserByIdQuery getQuery:
                    var queriedUser = await _userService.GetUserByIdAsync(getQuery.Id, cancellationToken);
                    envelope.Sender.Tell(new GetUserByIdQuery.Reply {
                        State = queriedUser
                    });
                    break;
                default:
                    envelope.Sender.Tell(new FailureReply(ExceptionDispatchInfo.Capture(new ArgumentException("Unhandled message"))));
                    break;
                // Add more cases for other commands as needed
            }
        } catch (Exception ex) {
            envelope.Sender.Tell(new FailureReply(ExceptionDispatchInfo.Capture(ex)));
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
    public required int Id { get; init; }

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

