using System.Collections.Concurrent;
using System.Threading.Channels;
using ChatApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ChatApp.Domain.Services;

public class UserService : IActorService
{
    private readonly ConcurrentDictionary<int, User> _users = new();
    private readonly Channel<UserCommand> _userChannel = Channel.CreateUnbounded<UserCommand>();
    private readonly ILogger<UserService> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public async Task<User> CreateUserAsync(string username)
    {
        var user = new User { Id = _users.Count + 1, Username = username };
        var command = new CreateUserCommand(user);
        await _userChannel.Writer.WriteAsync(command);
        return user;
    }

    public User? GetUser(int id)
    {
        _users.TryGetValue(id, out var user);
        return user;
    }

    public IEnumerable<User> GetAllUsers()
    {
        return _users.Values;
    }

    public async Task ProcessUsersAsync()
    {
        await foreach (var command in _userChannel.Reader.ReadAllAsync(_cancellationTokenSource.Token))
        {
            switch (command)
            {
                case CreateUserCommand createUserCommand:
                    _users[createUserCommand.User.Id] = createUserCommand.User;
                    _logger.LogInformation($"Processing user: {createUserCommand.User.Username}");
                    break;
                // Add more cases for other commands as needed
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(ProcessUsersAsync, cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }
}

public abstract class UserCommand { }

public class CreateUserCommand : UserCommand
{
    public User User { get; }

    public CreateUserCommand(User user)
    {
        User = user;
    }
}
