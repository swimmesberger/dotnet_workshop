namespace ChatApp.Domain.Entities;

public class Message
{
    public int Id { get; set; }
    public int ChatRoomId { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
}
