using ChatApp.Application.Domain.ChatRooms;
using ChatApp.Common.Actor.Abstractions;

namespace ChatApp.Application.ChatRooms;

public sealed class ChatRoomManagementActor : IActor {
    private readonly ChatRoomManagementService _chatRoomService;

    public ChatRoomManagementActor(ChatRoomManagementService chatRoomService) {
        _chatRoomService = chatRoomService;
    }

    public async ValueTask OnLetter(Envelope letter) {
        try {
            switch (letter.Body) {
                case InitiateCommand:
                case PassivateCommand:
                    letter.Sender.Tell(SuccessReply.Instance);
                    break;
                case CreateChatRoomCommand createCommand:
                    var createdChatRoom = await _chatRoomService.CreateChatRoomAsync(createCommand.Name, letter.CancellationToken);
                    letter.Sender.Tell(new CreateChatRoomCommand.Reply {
                        State = createdChatRoom
                    });
                    break;
                case GetAllChatRoomIdsQuery:
                    var queriedChatRooms = await _chatRoomService.GetAllChatRoomIdsAsync(letter.CancellationToken);
                    letter.Sender.Tell(new GetAllChatRoomIdsQuery.Reply {
                        State = queriedChatRooms
                    });
                    break;
                default:
                    letter.Sender.Tell(new FailureReply(new ArgumentException("Unhandled message")));
                    break;
                // Add more cases for other commands as needed
            }
        } catch (Exception ex) {
            letter.Sender.Tell(new FailureReply(ex));
        }
    }
}


public sealed record CreateChatRoomCommand : IRequest<ChatRoom, CreateChatRoomCommand.Reply> {
    public required string Name { get; init; }

    public sealed class Reply : IReply<ChatRoom> {
        public required ChatRoom State { get; init; }
    }
}

public sealed record GetAllChatRoomIdsQuery : IRequest<List<int>, GetAllChatRoomIdsQuery.Reply> {
    public sealed class Reply : IReply<List<int>> {
        public required List<int> State { get; init; }
    }
}

