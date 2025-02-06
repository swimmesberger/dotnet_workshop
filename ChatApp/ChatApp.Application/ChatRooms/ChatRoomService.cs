using ChatApp.Common;
using ChatApp.Domain.ChatRooms;
using Microsoft.Extensions.Logging;

namespace ChatApp.Application.ChatRooms;

public sealed class ChatRoomState {
    public Dictionary<int, ChatRoom> ChatRooms { get; init; } = new Dictionary<int, ChatRoom>();
}

public sealed class ChatRoomService {
    private readonly ILogger<ChatRoomService> _logger;
    private readonly IStorage<ChatRoomState> _storage;

    private ChatRoomState State => _storage.State;

    private Dictionary<int, ChatRoom> ChatRooms => State.ChatRooms;

    public ChatRoomService(ILogger<ChatRoomService> logger, IStorage<ChatRoomState> storage) {
        _logger = logger;
        _storage = storage;
    }

    public async Task<ChatRoom> CreateChatRoomAsync(string name, CancellationToken cancellationToken = default) {
        var chatRoom = new ChatRoom {
            Id = ChatRooms.Count + 1,
            Name = name
        };
        _logger.LogInformation($"Processing chat room: {chatRoom.Name}");
        ChatRooms[chatRoom.Id] = chatRoom;
        await _storage.SaveStateAsync(cancellationToken);
        return new ChatRoom(chatRoom);
    }

    public Task<ChatRoom?> GetChatRoomByIdAsync(int id, CancellationToken cancellationToken = default) {
        ChatRooms.TryGetValue(id, out var chatRoom);
        return Task.FromResult(chatRoom == null ? null : new ChatRoom(chatRoom));
    }

    public async Task<ChatMessage> SendMessageAsync(int chatRoomId, int senderUserId, string content, CancellationToken cancellationToken = default) {
        if (!ChatRooms.TryGetValue(chatRoomId, out var chatRoom)) {
            throw new ArgumentException("Invalid chat room ID");
        }
        var message = new ChatMessage {
            Id = chatRoom.Messages.Count + 1,
            ChatRoomId = chatRoomId,
            SenderUserId = senderUserId,
            Content = content,
            Timestamp = DateTimeOffset.UtcNow
        };
        _logger.LogInformation($"Processing message from user {message.SenderUserId} in chat room {message.ChatRoomId}: {message.Content}");
        chatRoom.Messages.Add(message);
        await _storage.SaveStateAsync(cancellationToken);
        return new ChatMessage(message);
    }

    public Task<List<ChatRoom>> GetAllChatRoomsAsync(CancellationToken cancellationToken = default) {
        return Task.FromResult(ChatRooms.Values.Select(x => new ChatRoom(x)).ToList());
    }

    public Task<List<ChatMessage>> GetAllMessagesByRoomIdAsync(int chatRoomId, CancellationToken cancellationToken = default) {
        if (!ChatRooms.TryGetValue(chatRoomId, out var chatRoom)) {
            throw new ArgumentException("Invalid chat room ID");
        }
        return Task.FromResult(chatRoom.Messages.Select(x => new ChatMessage(x)).ToList());
    }
}
