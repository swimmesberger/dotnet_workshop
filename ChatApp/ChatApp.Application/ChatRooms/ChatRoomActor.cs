using ChatApp.Application.Domain.ChatRooms;
using ChatApp.Common.Actors.Abstractions;

namespace ChatApp.Application.ChatRooms;

public sealed class ChatRoomActor : IActor {
    private IActorContext Context { get; }
    private readonly ChatRoomService _chatRoomService;

    public ChatRoomActor(IActorContext context, ChatRoomService chatRoomService) {
        Context = context;
        _chatRoomService = chatRoomService;
    }

    public async ValueTask OnLetter() {
        try {
            switch (Context.Letter.Body) {
                case InitiateCommand or PassivateCommand:
                    Context.Letter.Sender.Tell(SuccessReply.Instance);
                    break;
                case InitializeChatRoomCommand initializeCommand:
                    await _chatRoomService.InitializeAsync(initializeCommand.State, Context.RequestAborted);
                    Context.Letter.Sender.Tell(SuccessReply.Instance);
                    break;
                case JoinChatRoomCommand joinCommand:
                    var joinedChatRoom = await _chatRoomService.JoinAsync(joinCommand.UserId, Context.RequestAborted);
                    Context.Letter.Sender.Tell(new JoinChatRoomCommand.Reply {
                        State = joinedChatRoom
                    });
                    break;
                case SendChatRoomMessageCommand sendMessageCommand:
                    var message = await _chatRoomService.SendMessageAsync(sendMessageCommand.SenderUserId,
                        sendMessageCommand.Content, Context.RequestAborted);
                    Context.Letter.Sender.Tell(new SendChatRoomMessageCommand.Reply {
                        State = message
                    });
                    break;
                case GetAllChatRoomMessagesQuery:
                    var messages = await _chatRoomService.GetAllMessagesAsync(Context.RequestAborted);
                    Context.Letter.Sender.Tell(new GetAllChatRoomMessagesQuery.Reply {
                        State = messages
                    });
                    break;
                case GetChatRoomQuery:
                    var queriedChatRoom = await _chatRoomService.GetAsync(Context.RequestAborted);
                    Context.Letter.Sender.Tell(new GetChatRoomQuery.Reply {
                        State = queriedChatRoom
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

public sealed class SendChatRoomMessageCommand : IRequest<ChatMessage, SendChatRoomMessageCommand.Reply> {
    public required int SenderUserId { get; init; }
    public required string Content { get; init; }


    public sealed class Reply : IReply<ChatMessage> {
        public required ChatMessage State { get; init; }
    }
}

public sealed class InitializeChatRoomCommand : IRequest {
    public required ChatRoom State { get; init; }
}

public sealed record GetAllChatRoomMessagesQuery : IRequest<List<ChatMessage>, GetAllChatRoomMessagesQuery.Reply> {
    public sealed class Reply : IReply<List<ChatMessage>> {
        public required List<ChatMessage> State { get; init; }
    }
}

public sealed record GetChatRoomQuery : IRequest<ChatRoom?, GetChatRoomQuery.Reply> {
    public sealed class Reply : IReply<ChatRoom?> {
        public required ChatRoom? State { get; init; }
    }
}

public sealed record JoinChatRoomCommand : IRequest<ChatRoom, JoinChatRoomCommand.Reply> {
    public required int UserId { get; init; }

    public sealed class Reply : IReply<ChatRoom> {
        public required ChatRoom State { get; init; }
    }
}
