namespace ChatApp.Domain.Entities;

public class ChatRoom
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
