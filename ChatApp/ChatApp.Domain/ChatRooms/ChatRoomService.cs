using Microsoft.Extensions.Logging;

namespace ChatApp.Domain.ChatRooms;

public sealed class ChatRoomService : IChatRoomService {
    private readonly ILogger<ChatRoomService> _logger;
    private readonly Dictionary<int, ChatRoom> _chatRooms = new();

    public ChatRoomService(ILogger<ChatRoomService> logger) {
        _logger = logger;
    }

    public Task<ChatRoom> CreateChatRoomAsync(string name, CancellationToken cancellationToken = default) {
        var chatRoom = new ChatRoom {
            Id = _chatRooms.Count + 1,
            Name = name
        };
        _chatRooms[chatRoom.Id] = chatRoom;
        _logger.LogInformation($"Processing chat room: {chatRoom.Name}");
        return Task.FromResult(new ChatRoom(chatRoom));
    }

    public Task<ChatRoom?> GetChatRoomByIdAsync(int id, CancellationToken cancellationToken = default) {
        _chatRooms.TryGetValue(id, out var chatRoom);
        return Task.FromResult(chatRoom == null ? null : new ChatRoom(chatRoom));
    }

    public Task<ChatMessage> SendMessageAsync(int chatRoomId, int senderUserId, string content, CancellationToken cancellationToken = default) {
        if (!_chatRooms.TryGetValue(chatRoomId, out var chatRoom)) {
            throw new ArgumentException("Invalid chat room ID");
        }
        var message = new ChatMessage {
            Id = chatRoom.Messages.Count + 1,
            ChatRoomId = chatRoomId,
            SenderUserId = senderUserId,
            Content = content,
            Timestamp = DateTimeOffset.UtcNow
        };
        chatRoom.Messages.Add(message);
        _logger.LogInformation($"Processing message from user {message.SenderUserId} in chat room {message.ChatRoomId}: {message.Content}");
        return Task.FromResult(new ChatMessage(message));
    }

    public Task<List<ChatRoom>> GetAllChatRoomsAsync(CancellationToken cancellationToken = default) {
        return Task.FromResult(_chatRooms.Values.Select(x => new ChatRoom(x)).ToList());
    }

    public Task<List<ChatMessage>> GetAllMessagesByRoomIdAsync(int chatRoomId, CancellationToken cancellationToken = default) {
        if (!_chatRooms.TryGetValue(chatRoomId, out var chatRoom)) {
            throw new ArgumentException("Invalid chat room ID");
        }
        return Task.FromResult(chatRoom.Messages.Select(x => new ChatMessage(x)).ToList());
    }
}
