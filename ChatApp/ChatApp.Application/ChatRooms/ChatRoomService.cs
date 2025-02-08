using ChatApp.Application.Common;
using ChatApp.Application.Domain.ChatRooms;
using Microsoft.Extensions.Logging;

namespace ChatApp.Application.ChatRooms;

public sealed class ChatRoomService {
    private readonly ILogger<ChatRoomService> _logger;
    private readonly IStorage<ChatRoom> _storage;

    private ChatRoom State {
        get => _storage.State ?? throw new ArgumentException("Chat room state is not initialized");
        set => _storage.State = value;
    }

    public ChatRoomService(ILogger<ChatRoomService> logger, IStorage<ChatRoom> storage) {
        _logger = logger;
        _storage = storage;
    }

    public async Task InitializeAsync(ChatRoom chatRoom, CancellationToken cancellationToken = default) {
        if (_storage.RecordExists) {
            // Already initialized
           return;
        }
        State = new ChatRoom(chatRoom);
        await _storage.SaveStateAsync(cancellationToken);
    }

    public async Task<ChatMessage> SendMessageAsync(int senderUserId, string content, CancellationToken cancellationToken = default) {
        var message = new ChatMessage {
            Id = State.Messages.Count + 1,
            ChatRoomId = State.Id,
            SenderUserId = senderUserId,
            Content = content,
            Timestamp = DateTimeOffset.UtcNow
        };
        _logger.LogInformation("Processing message from user {MessageSenderUserId} in chat room {MessageChatRoomId}: {MessageContent}", message.SenderUserId, message.ChatRoomId, message.Content);
        State.Messages.Add(message);
        await _storage.SaveStateAsync(cancellationToken);
        return new ChatMessage(message);
    }

    public Task<ChatRoom> GetAsync(CancellationToken cancellationToken = default) {
        return Task.FromResult(new ChatRoom(State));
    }

    public Task<List<ChatMessage>> GetAllMessagesAsync(CancellationToken cancellationToken = default) {
        return Task.FromResult(State.Messages.Select(x => new ChatMessage(x)).ToList());
    }
}
