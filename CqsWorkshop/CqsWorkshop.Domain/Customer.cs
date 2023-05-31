namespace CqsWorkshop.Domain;

public class Customer {
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public Address? Address { get; set; }

    public Rating Rating { get; set; }

    public decimal? TotalRevenue { get; set; }

    public IList<Order> Orders { get; set; }
    
    public DateTimeOffset CreatedAt { get; private init; }
    
    public Customer(string name, Rating rating) : this(Guid.NewGuid(), name, rating) { }

    public Customer(Guid id, string name, Rating rating) {
        Id = id;
        Name = name;
        Rating = rating;
        CreatedAt = DateTimeOffset.UtcNow;
        Orders = new List<Order>();
    }

    public void AddOrders(params Order[] orders) {
        foreach (Order order in orders) {
            order.AssignCustomer(this);
        }
    }

    public override string ToString() {
        return $"Customer {{ Id: {Id}, Name: {Name}, TotalRevenue: {TotalRevenue} }}";
    }
}