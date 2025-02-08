using ChatApp.Application.Common;
using ChatApp.Application.Domain.ChatRooms;
using ChatApp.Application.Users;
using Microsoft.Extensions.Logging;

namespace ChatApp.Application.ChatRooms;

public sealed class ChatRoomService {
    private readonly ILogger<ChatRoomService> _logger;
    private readonly IStorage<ChatRoom> _storage;
    private readonly UserClient _userClient;

    private ChatRoom State {
        get => _storage.State ?? throw new ArgumentException("Chat room state is not initialized");
        set => _storage.State = value;
    }

    public ChatRoomService(ILogger<ChatRoomService> logger, IStorage<ChatRoom> storage, UserClient userClient) {
        _logger = logger;
        _storage = storage;
        _userClient = userClient;
    }

    public async Task InitializeAsync(ChatRoom chatRoom, CancellationToken cancellationToken = default) {
        if (_storage.RecordExists) {
            // Already initialized
           return;
        }
        State = new ChatRoom(chatRoom);
        await _storage.SaveStateAsync(cancellationToken);
    }

    public async Task<ChatRoom> JoinAsync(int userId, CancellationToken cancellationToken = default) {
        if (State.Users.Any(x => x.Id == userId)) {
            return new ChatRoom(State);
        }
        var user = await _userClient.GetUserByIdAsync(userId, cancellationToken: cancellationToken);
        if (user is null) {
            throw new ArgumentException("User not found");
        }
        State.Users.Add(user);
        await _storage.SaveStateAsync(cancellationToken);
        return new ChatRoom(State);
    }

    public async Task<ChatMessage> SendMessageAsync(int senderUserId, string content, CancellationToken cancellationToken = default) {
        if (State.Users.All(x => x.Id != senderUserId)) {
            throw new ArgumentException("User is not in the chat room");
        }
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
