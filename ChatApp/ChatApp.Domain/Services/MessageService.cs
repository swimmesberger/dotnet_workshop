using System.Collections.Concurrent;
using System.Threading.Channels;
using ChatApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ChatApp.Domain.Services;

public class MessageService : IActorService
{
    private readonly ConcurrentDictionary<int, Message> _messages = new();
    private readonly Channel<MessageCommand> _messageChannel = Channel.CreateUnbounded<MessageCommand>();
    private readonly ILogger<MessageService> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public MessageService(ILogger<MessageService> logger)
    {
        _logger = logger;
    }

    public async Task<Message> SendMessageAsync(int chatRoomId, int userId, string content)
    {
        var message = new Message
        {
            Id = _messages.Count + 1,
            ChatRoomId = chatRoomId,
            UserId = userId,
            Content = content,
            Timestamp = DateTime.UtcNow
        };
        var command = new SendMessageCommand(message);
        await _messageChannel.Writer.WriteAsync(command);
        return message;
    }

    public Message? GetMessage(int id)
    {
        _messages.TryGetValue(id, out var message);
        return message;
    }

    public IEnumerable<Message> GetAllMessages()
    {
        return _messages.Values;
    }

    public async Task ProcessMessagesAsync()
    {
        await foreach (var command in _messageChannel.Reader.ReadAllAsync(_cancellationTokenSource.Token))
        {
            switch (command)
            {
                case SendMessageCommand sendMessageCommand:
                    _messages[sendMessageCommand.Message.Id] = sendMessageCommand.Message;
                    _logger.LogInformation($"Processing message from user {sendMessageCommand.Message.UserId} in chat room {sendMessageCommand.Message.ChatRoomId}: {sendMessageCommand.Message.Content}");
                    break;
                // Add more cases for other commands as needed
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(ProcessMessagesAsync, cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }
}

public abstract class MessageCommand { }

public class SendMessageCommand : MessageCommand
{
    public Message Message { get; }

    public SendMessageCommand(Message message)
    {
        Message = message;
    }
}
