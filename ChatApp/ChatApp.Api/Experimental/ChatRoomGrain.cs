using ChatApp.Application.Domain.ChatRooms;
using ChatApp.Common.Grains;

namespace ChatApp.Api.Experimental;

public sealed class ChatRoomGrain : IGrain {
    public Task<ChatRoom> JoinChatRoomAsync(int chatRoomId, int userId, CancellationToken cancellationToken = default) {
        return Task.FromResult(new ChatRoom());
    }

    public Task RefreshChatRoomAsync(int chatRoomId, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }
}
