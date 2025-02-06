using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace ChatApp.Domain.Users;

public sealed class UserService : IUserService {
    private readonly ILogger<UserService> _logger;
    private readonly ConcurrentDictionary<int, User> _users = new();

    public UserService(ILogger<UserService> logger) {
        _logger = logger;
    }

    public Task<User> CreateUserAsync(string username, CancellationToken cancellationToken = default) {
        var user = new User {
            Id = _users.Count + 1,
            Username = username
        };
        _users[user.Id] = user;
        _logger.LogInformation($"Processing user: {user.Username}");
        return Task.FromResult(new User(user));
    }

    public Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default) {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user == null ? null : new User(user));
    }

    public Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default) {
        return Task.FromResult(_users.Values.Select(x => new User(x)).ToList());
    }
}
