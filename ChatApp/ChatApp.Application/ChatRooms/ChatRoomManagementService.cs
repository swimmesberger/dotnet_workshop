using ChatApp.Application.Common;
using ChatApp.Application.Domain.ChatRooms;
using ChatApp.Common.Actor.Abstractions;
using Microsoft.Extensions.Logging;

namespace ChatApp.Application.ChatRooms;

public sealed class ChatRoomManagementState {
    public List<int> ChatRooms { get; init; } = new List<int>();
}

public sealed class ChatRoomManagementService {
    private readonly ILogger<ChatRoomManagementService> _logger;
    private readonly IStorage<ChatRoomManagementState> _storage;
    private readonly IActorSystem _actorSystem;

    private ChatRoomManagementState State {
        get => _storage.State;
        set => _storage.State = value;
    }
    private List<int> ChatRooms => State.ChatRooms;

    public ChatRoomManagementService(ILogger<ChatRoomManagementService> logger, IStorage<ChatRoomManagementState> storage, IActorSystem actorSystem) {
        _logger = logger;
        _storage = storage;
        _actorSystem = actorSystem;
    }

    public async Task<ChatRoom> CreateChatRoomAsync(string name, CancellationToken cancellationToken = default) {
        var chatRoom = new ChatRoom {
            Id = ChatRooms.Count + 1,
            Name = name
        };
        _logger.LogInformation("Processing chat room: {ChatRoomName}", chatRoom.Name);

        var actorRef = await _actorSystem.CreateActorAsync(new ActorConfiguration<ChatRoomActor> {
            Id = chatRoom.Id.ToString()
        }, cancellationToken);
        await actorRef.Ask(new InitializeChatRoomCommand {
            State = chatRoom
        }, cancellationToken: cancellationToken);
        ChatRooms.Add(chatRoom.Id);
        await _storage.SaveStateAsync(cancellationToken);

        return new ChatRoom(chatRoom);
    }

    public Task<List<int>> GetAllChatRoomIdsAsync(CancellationToken cancellationToken = default) {
        return Task.FromResult(new List<int>(ChatRooms));
    }
}
