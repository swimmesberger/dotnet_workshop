namespace ChatApp.Application.Domain.ChatRooms;

public sealed class ChatMessage {
    public int Id { get; set; }
    public int ChatRoomId { get; set; }
    public int SenderUserId { get; set; }
    public string Content { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    public ChatMessage() { }

    public ChatMessage(ChatMessage chatMessage) {
        Id = chatMessage.Id;
        ChatRoomId = chatMessage.ChatRoomId;
        SenderUserId = chatMessage.SenderUserId;
        Content = chatMessage.Content;
        Timestamp = chatMessage.Timestamp;
    }
}
