namespace ChatApp.Domain.Users;

public interface IUserService {
    Task<User> CreateUserAsync(string username, CancellationToken cancellationToken = default);

    Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}
