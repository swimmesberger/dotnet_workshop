namespace CqsWorkshop.Contract;

public sealed record CustomerForCreationDto {
    public required Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Rating Rating { get; init; }
    public AddressDto? Address { get; init; }
}
