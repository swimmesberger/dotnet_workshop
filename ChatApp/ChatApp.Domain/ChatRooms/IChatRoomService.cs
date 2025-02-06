namespace ChatApp.Domain.ChatRooms;

public interface IChatRoomService {
    Task<ChatRoom> CreateChatRoomAsync(string name, CancellationToken cancellationToken = default);

    Task<ChatRoom?> GetChatRoomByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ChatMessage> SendMessageAsync(int chatRoomId, int senderUserId, string content,
        CancellationToken cancellationToken = default);

    Task<List<ChatRoom>> GetAllChatRoomsAsync(CancellationToken cancellationToken = default);

    Task<List<ChatMessage>> GetAllMessagesByRoomIdAsync(int chatRoomId, CancellationToken cancellationToken = default);
}
