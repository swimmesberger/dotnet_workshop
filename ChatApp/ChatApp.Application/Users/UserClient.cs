using ChatApp.Application.Domain.Users;
using ChatApp.Common.Actors.Abstractions;

namespace ChatApp.Application.Users;

public sealed class UserClient {
    private readonly IActorRef _actorRef;

    public UserClient(IRequiredActor<UserManagementActor> actor) {
        _actorRef = actor.ActorRef;
    }

    public async Task<User> CreateUserAsync(string username, ClientRequestOptions? options = null, CancellationToken cancellationToken = default) {
        return await _actorRef.Ask(new CreateUserCommand {
            Username = username
        }, new RequestOptions {
            Headers = options?.Headers
        }, cancellationToken: cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(int id, ClientRequestOptions? options = null,CancellationToken cancellationToken = default) {
        return await _actorRef.Ask(new GetUserByIdQuery {
            Id = id
        }, new RequestOptions {
            Headers = options?.Headers
        }, cancellationToken: cancellationToken);
    }

    public async Task<List<User>> GetAllUsersAsync(ClientRequestOptions? options = null, CancellationToken cancellationToken = default) {
        return await _actorRef.Ask(new GetAllUsersQuery(), new RequestOptions {
            Headers = options?.Headers
        }, cancellationToken: cancellationToken);
    }
}
