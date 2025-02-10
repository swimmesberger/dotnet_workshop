using ChatApp.Application.Domain.ChatRooms;
using ChatApp.Common.Actors.Abstractions;
using ChatApp.Common.Actors.Local;

namespace ChatApp.Application.ChatRooms;

public sealed class ChatRoomManagementActor : IActor {
    private IActorContext Context { get; }
    private readonly ChatRoomManagementService _chatRoomService;

    public ChatRoomManagementActor(IActorContext context, ChatRoomManagementService chatRoomService) {
        Context = context;
        _chatRoomService = chatRoomService;
    }

    public async ValueTask OnLetter() {
        try {
            switch (Context.Letter.Body) {
                case InitiateCommand:
                case PassivateCommand:
                    Context.Letter.Sender.Tell(SuccessReply.Instance);
                    break;
                case CreateChatRoomCommand createCommand:
                    var createdChatRoom = await _chatRoomService.CreateChatRoomAsync(createCommand.Name, Context.RequestAborted);
                    Context.Letter.Sender.Tell(new CreateChatRoomCommand.Reply {
                        State = createdChatRoom
                    });
                    break;
                case GetAllChatRoomIdsQuery:
                    var queriedChatRooms = await _chatRoomService.GetAllChatRoomIdsAsync(Context.RequestAborted);
                    Context.Letter.Sender.Tell(new GetAllChatRoomIdsQuery.Reply {
                        State = queriedChatRooms
                    });
                    break;
                default:
                    Context.Letter.Sender.Tell(new FailureReply(new ArgumentException("Unhandled message")));
                    break;
                // Add more cases for other commands as needed
            }
        } catch (Exception ex) {
            Context.Letter.Sender.Tell(new FailureReply(ex));
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

