using System.Collections.Concurrent;
using System.Threading.Channels;
using ChatApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ChatApp.Domain.Services;

public class ChatRoomService : IActorService
{
    private readonly ConcurrentDictionary<int, ChatRoom> _chatRooms = new();
    private readonly Channel<ChatRoomCommand> _chatRoomChannel = Channel.CreateUnbounded<ChatRoomCommand>();
    private readonly ILogger<ChatRoomService> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ChatRoomService(ILogger<ChatRoomService> logger)
    {
        _logger = logger;
    }

    public async Task<ChatRoom> CreateChatRoomAsync(string name)
    {
        var chatRoom = new ChatRoom { Id = _chatRooms.Count + 1, Name = name };
        var command = new CreateChatRoomCommand(chatRoom);
        await _chatRoomChannel.Writer.WriteAsync(command);
        return chatRoom;
    }

    public ChatRoom? GetChatRoom(int id)
    {
        _chatRooms.TryGetValue(id, out var chatRoom);
        return chatRoom;
    }

    public IEnumerable<ChatRoom> GetAllChatRooms()
    {
        return _chatRooms.Values;
    }

    public async Task ProcessChatRoomsAsync()
    {
        await foreach (var command in _chatRoomChannel.Reader.ReadAllAsync(_cancellationTokenSource.Token))
        {
            switch (command)
            {
                case CreateChatRoomCommand createCommand:
                    _chatRooms[createCommand.ChatRoom.Id] = createCommand.ChatRoom;
                    _logger.LogInformation($"Processing chat room: {createCommand.ChatRoom.Name}");
                    break;
                // Add more cases for other commands as needed
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(ProcessChatRoomsAsync, cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }
}

public abstract class ChatRoomCommand { }

public class CreateChatRoomCommand : ChatRoomCommand
{
    public ChatRoom ChatRoom { get; }

    public CreateChatRoomCommand(ChatRoom chatRoom)
    {
        ChatRoom = chatRoom;
    }
}
