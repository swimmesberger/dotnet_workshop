using ChatApp.Application.Domain.ChatRooms;
using ChatApp.Common.Actor.Abstractions;

namespace ChatApp.Application.ChatRooms;

public sealed class ChatRoomClient {
    private readonly IActorRef _actorRef;

    public ChatRoomClient(IRequiredActor<ChatRoomActor> actor) {
        _actorRef = actor.ActorRef;
    }

    public async Task<ChatRoom> CreateChatRoomAsync(string name, ClientRequestOptions? options = null, CancellationToken cancellationToken = default) {
        return await _actorRef.Ask(new CreateChatRoomCommand {
            Name = name
        }, new RequestOptions {
            Headers = options?.Headers
        }, cancellationToken: cancellationToken);
    }

    public async Task<ChatRoom?> GetChatRoomByIdAsync(int id, ClientRequestOptions? options = null, CancellationToken cancellationToken = default) {
        return await _actorRef.Ask(new GetChatRoomByIdQuery {
            Id = id
        }, new RequestOptions {
            Headers = options?.Headers
        }, cancellationToken: cancellationToken);
    }

    public async Task<ChatMessage> SendMessageAsync(int chatRoomId, int senderUserId, string content, ClientRequestOptions? options = null, CancellationToken cancellationToken = default) {
        return await _actorRef.Ask(new SendMessageCommand {
            ChatRoomId = chatRoomId,
            SenderUserId = senderUserId,
            Content = content
        }, new RequestOptions {
            Headers = options?.Headers
        }, cancellationToken: cancellationToken);
    }

    public async Task<List<ChatRoom>> GetAllChatRoomsAsync(ClientRequestOptions? options = null, CancellationToken cancellationToken = default) {
        return await _actorRef.Ask(new GetAllChatRoomsQuery(), new RequestOptions {
            Headers = options?.Headers
        }, cancellationToken: cancellationToken);
    }

    public async Task<List<ChatMessage>> GetAllMessagesByRoomIdAsync(int chatRoomId, ClientRequestOptions? options = null, CancellationToken cancellationToken = default) {
        return await _actorRef.Ask(new GetAllMessageByRoomIdQuery {
            ChatRoomId = chatRoomId
        }, new RequestOptions {
            Headers = options?.Headers
        }, cancellationToken: cancellationToken);
    }
}
