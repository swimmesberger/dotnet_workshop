namespace CqsWorkshop.Domain;

public class Order {
    public Guid Id { get; set; }

    public string Article { get; set; }

    public DateTimeOffset OrderDate { get; set; }

    public decimal TotalPrice { get; set; }

    public Customer? Customer { get; set; }
    
    public Order(string article, DateTimeOffset orderDate, decimal totalPrice) :
        this(Guid.NewGuid(), article, orderDate, totalPrice) { }

    public Order(Guid id, string article, DateTimeOffset orderDate, decimal totalPrice) {
        Id = id;
        OrderDate = orderDate;
        Article = article;
        TotalPrice = totalPrice;
    }

    public void AssignCustomer(Customer customer) {
        Customer = customer;
        customer.Orders.Add(this);
    }

    public override string ToString() {
        return
            $"Order {{ Id: {Id}, Article: {Article}, OrderDate: {OrderDate}, TotalPrice: {TotalPrice}, Customer: {Customer?.Name} }}";
    }
}