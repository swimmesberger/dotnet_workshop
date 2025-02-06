using ChatApp.Common.Actors;
using ChatApp.Domain.ChatRooms;

namespace ChatApp.Application.ChatRooms;

public sealed class ChatRoomClient : IChatRoomService {
    private readonly IActorRef _actorRef;

    public ChatRoomClient(IRequiredActor<ChatRoomActor> actor) {
        _actorRef = actor.ActorRef;
    }

    public async Task<ChatRoom> CreateChatRoomAsync(string name, CancellationToken cancellationToken = default) {
        return await _actorRef.Ask(new CreateChatRoomCommand {
            Name = name
        }, cancellationToken: cancellationToken);
    }

    public async Task<ChatRoom?> GetChatRoomByIdAsync(int id, CancellationToken cancellationToken = default) {
        return await _actorRef.Ask(new GetChatRoomByIdQuery {
            Id = id
        }, cancellationToken: cancellationToken);
    }

    public async Task<ChatMessage> SendMessageAsync(int chatRoomId, int senderUserId, string content, CancellationToken cancellationToken = default) {
        return await _actorRef.Ask(new SendMessageCommand {
            ChatRoomId = chatRoomId,
            SenderUserId = senderUserId,
            Content = content
        }, cancellationToken: cancellationToken);
    }

    public async Task<List<ChatRoom>> GetAllChatRoomsAsync(CancellationToken cancellationToken = default) {
        return await _actorRef.Ask(new GetAllChatRoomsQuery(), cancellationToken: cancellationToken);
    }

    public async Task<List<ChatMessage>> GetAllMessagesByRoomIdAsync(int chatRoomId, CancellationToken cancellationToken = default) {
        return await _actorRef.Ask(new GetAllMessageByRoomIdQuery {
            ChatRoomId = chatRoomId
        }, cancellationToken: cancellationToken);
    }
}
