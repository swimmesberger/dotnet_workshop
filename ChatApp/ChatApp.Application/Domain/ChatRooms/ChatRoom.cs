namespace ChatApp.Application.Domain.ChatRooms;

public class ChatRoom {
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

    public ChatRoom() { }

    public ChatRoom(ChatRoom chatRoom) {
        Id = chatRoom.Id;
        Name = chatRoom.Name;
        Messages = chatRoom.Messages.Select(m => new ChatMessage(m)).ToList();
    }
}
