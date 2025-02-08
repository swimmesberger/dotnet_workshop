using ChatApp.Application.Domain;
using ChatApp.Application.Domain.ChatRooms;
using ChatApp.Common.Actor.Abstractions;

namespace ChatApp.Application.ChatRooms;

public sealed class ChatRoomClient {
    private readonly IActorSystem _actorSystem;
    private readonly IActorRef _managementActorRef;

    public ChatRoomClient(IActorSystem actorSystem, IRequiredActor<ChatRoomManagementActor> managementActor) {
        _actorSystem = actorSystem;
        _managementActorRef = managementActor.ActorRef;
    }

    public async Task<ChatRoom> CreateChatRoomAsync(string name, ClientRequestOptions? options = null, CancellationToken cancellationToken = default) {
        return await _managementActorRef.Ask(new CreateChatRoomCommand {
            Name = name
        }, new RequestOptions {
            Headers = options?.Headers
        }, cancellationToken: cancellationToken);
    }

    public async Task<List<ChatRoom>> GetAllChatRoomsAsync(ClientRequestOptions? options = null, CancellationToken cancellationToken = default) {
        var chatRoomIds = await _managementActorRef.Ask(new GetAllChatRoomIdsQuery(), new RequestOptions {
            Headers = options?.Headers
        }, cancellationToken: cancellationToken);
        var results = await Task.WhenAll(chatRoomIds.Select(chatRoomId => GetChatRoomByIdAsync(chatRoomId, options, cancellationToken)).ToArray());
        return results.OfType<ChatRoom>().ToList();
    }

    public async Task<ChatRoom?> GetChatRoomByIdAsync(int chatRoomId, ClientRequestOptions? options = null, CancellationToken cancellationToken = default) {
        var chatRoomActor = await GetChatRoomActor(chatRoomId, cancellationToken);
        return await chatRoomActor.Ask(new GetChatRoomQuery(), new RequestOptions {
            Headers = options?.Headers
        }, cancellationToken: cancellationToken);
    }

    public async Task<ChatMessage> SendMessageAsync(int chatRoomId, int senderUserId, string content, ClientRequestOptions? options = null, CancellationToken cancellationToken = default) {
        var chatRoomActor = await GetChatRoomActor(chatRoomId, cancellationToken);
        return await chatRoomActor.Ask(new SendChatRoomMessageCommand {
            SenderUserId = senderUserId,
            Content = content
        }, new RequestOptions {
            Headers = options?.Headers
        }, cancellationToken: cancellationToken);
    }

    public async Task<List<ChatMessage>> GetAllMessagesByRoomIdAsync(int chatRoomId, ClientRequestOptions? options = null, CancellationToken cancellationToken = default) {
        var chatRoomActor = await GetChatRoomActor(chatRoomId, cancellationToken);
        return await chatRoomActor.Ask(new GetAllChatRoomMessagesQuery(), new RequestOptions {
            Headers = options?.Headers
        }, cancellationToken: cancellationToken);
    }

    private async ValueTask<IActorRef> GetChatRoomActor(int chatRoomId, CancellationToken cancellationToken = default) {
        return await _actorSystem.GetActorAsync<ChatRoomActor>(chatRoomId.ToString(), cancellationToken) ?? throw new EntityNotFoundException($"ChatRoom with id {chatRoomId} not found");
    }
}
