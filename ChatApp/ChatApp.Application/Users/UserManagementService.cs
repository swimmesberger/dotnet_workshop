using ChatApp.Application.Common;
using ChatApp.Application.Domain.Users;
using Microsoft.Extensions.Logging;

namespace ChatApp.Application.Users;

public sealed class UserState {
    public Dictionary<int, User> Users { get; init; } = new Dictionary<int, User>();
}

public sealed class UserManagementService {
    private readonly ILogger<UserManagementService> _logger;
    private readonly IStorage<UserState> _storage;

    private UserState State => _storage.State;
    private Dictionary<int, User> Users => State.Users;

    public UserManagementService(ILogger<UserManagementService> logger, IStorage<UserState> storage) {
        _logger = logger;
        _storage = storage;
    }

    public async Task<User> CreateUserAsync(string username, CancellationToken cancellationToken = default) {
        var user = new User {
            Id = Users.Count + 1,
            Username = username
        };
        _logger.LogInformation($"Processing user: {user.Username}");
        Users[user.Id] = user;
        await _storage.SaveStateAsync(cancellationToken);
        return new User(user);
    }

    public Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default) {
        Users.TryGetValue(id, out var user);
        return Task.FromResult(user == null ? null : new User(user));
    }

    public Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default) {
        return Task.FromResult(Users.Values.Select(x => new User(x)).ToList());
    }
}
