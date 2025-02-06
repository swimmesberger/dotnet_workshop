namespace ChatApp.Domain.Commands;

public class Command
{
    public string CommandType { get; set; }
    public object Payload { get; set; }
    public DateTime Timestamp { get; set; }

    public Command(string commandType, object payload)
    {
        CommandType = commandType;
        Payload = payload;
        Timestamp = DateTime.UtcNow;
    }
}
