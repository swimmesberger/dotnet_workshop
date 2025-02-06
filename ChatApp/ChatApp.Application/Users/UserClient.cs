using ChatApp.Common.Actors;
using ChatApp.Domain.Users;

namespace ChatApp.Application.Users;

public sealed class UserClient : IUserService {
    private readonly IActorRef _actorRef;

    public UserClient(IRequiredActor<UserActor> actor) {
        _actorRef = actor.ActorRef;
    }

    public async Task<User> CreateUserAsync(string username, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }

    public Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }
}
