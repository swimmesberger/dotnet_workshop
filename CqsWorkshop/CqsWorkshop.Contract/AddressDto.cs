namespace CqsWorkshop.Contract;

public record AddressDto {
    public required string ZipCode { get; init; }
    public string City { get; init; } = string.Empty;
    public string? Street { get; init; }
}
