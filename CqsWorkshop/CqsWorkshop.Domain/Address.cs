namespace CqsWorkshop.Domain;

public class Address {
    public string ZipCode { get; set; }
    public string City { get; set; }
    public string? Street { get; set; }
    
    public Address(string zipCode, string city, string? street = null) {
        ZipCode = zipCode;
        City = city;
        Street = street;
    }

    public override string ToString() {
        return $"Address {{ ZipCode: {ZipCode}, City: {City}, Street: {Street ?? ""} }}";
    }
}