namespace CqsWorkshop.Contract;

public sealed record CustomerDto {
  public Guid Id { get; init; }
  public string Name { get; init; } = string.Empty;
  public Rating Rating { get; init; }
  public decimal? TotalRevenue { get; init; }
  public AddressDto? Address { get; init; }
}
