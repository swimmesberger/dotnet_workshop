using ChatApp.Application.Domain.Users;

namespace ChatApp.Application.Domain.ChatRooms;

public class ChatRoom {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<User> Users { get; set; } = [];
    public List<ChatMessage> Messages { get; set; } = [];

    public ChatRoom() { }

    public ChatRoom(ChatRoom chatRoom) {
        Id = chatRoom.Id;
        Name = chatRoom.Name;
        Users = chatRoom.Users.Select(u => new User(u)).ToList();
        Messages = chatRoom.Messages.Select(m => new ChatMessage(m)).ToList();
    }
}
