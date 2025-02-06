﻿using ChatApp.Application.Domain.ChatRooms;
using ChatApp.Common.Actor.Abstractions;

namespace ChatApp.Application.ChatRooms;

public sealed class ChatRoomActor : IActor {
    private readonly ChatRoomService _chatRoomService;

    public ChatRoomActor(ChatRoomService chatRoomService) {
        _chatRoomService = chatRoomService;
    }

    public async ValueTask OnLetter(Envelope letter) {
        try {
            switch (letter.Body) {
                case CreateChatRoomCommand createCommand:
                    var createdChatRoom = await _chatRoomService.CreateChatRoomAsync(createCommand.Name, letter.CancellationToken);
                    letter.Sender.Tell(new CreateChatRoomCommand.Reply {
                        State = createdChatRoom
                    });
                    break;
                case GetAllChatRoomsQuery:
                    var queriedChatRooms = await _chatRoomService.GetAllChatRoomsAsync(letter.CancellationToken);
                    letter.Sender.Tell(new GetAllChatRoomsQuery.Reply {
                        State = queriedChatRooms
                    });
                    break;
                case GetChatRoomByIdQuery getQuery:
                    var queriedChatRoom = await _chatRoomService.GetChatRoomByIdAsync(getQuery.Id, letter.CancellationToken);
                    letter.Sender.Tell(new GetChatRoomByIdQuery.Reply {
                        State = queriedChatRoom
                    });
                    break;
                case SendMessageCommand sendMessageCommand:
                    var message = await _chatRoomService.SendMessageAsync(sendMessageCommand.ChatRoomId, sendMessageCommand.SenderUserId,
                        sendMessageCommand.Content, letter.CancellationToken);
                    letter.Sender.Tell(new SendMessageCommand.Reply {
                        State = message
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

public sealed record GetAllChatRoomsQuery : IRequest<List<ChatRoom>, GetAllChatRoomsQuery.Reply> {
    public sealed class Reply : IReply<List<ChatRoom>> {
        public required List<ChatRoom> State { get; init; }
    }
}

public sealed record GetChatRoomByIdQuery : IRequest<ChatRoom?, GetChatRoomByIdQuery.Reply> {
    public required int Id { get; init; }

    public sealed class Reply : IReply<ChatRoom?> {
        public required ChatRoom? State { get; init; }
    }
}

public sealed class SendMessageCommand : IRequest<ChatMessage, SendMessageCommand.Reply> {
    public required int ChatRoomId { get; init; }
    public required int SenderUserId { get; init; }
    public required string Content { get; init; }


    public sealed class Reply : IReply<ChatMessage> {
        public required ChatMessage State { get; init; }
    }
}

public sealed record GetAllMessageByRoomIdQuery : IRequest<List<ChatMessage>, GetAllMessageByRoomIdQuery.Reply> {
    public required int ChatRoomId { get; init; }

    public sealed class Reply : IReply<List<ChatMessage>> {
        public required List<ChatMessage> State { get; init; }
    }
}
