namespace ChatApp.Domain.Users;

public sealed class User {
    public int Id { get; set; }
    public string Username { get; set; }

    public User() { }

    public User(User user) {
        Id = user.Id;
        Username = user.Username;
    }
}
