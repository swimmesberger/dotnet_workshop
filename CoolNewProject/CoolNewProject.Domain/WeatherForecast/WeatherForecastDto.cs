namespace CoolNewProject.Domain.WeatherForecast;

public sealed record WeatherForecastDto(Guid Id, DateOnly Date, int TemperatureC, string? Summary, DateTimeOffset CreatedAt) {
    public WeatherForecastDto() :
        this(DateOnly.MinValue, default, default) {}

    public WeatherForecastDto(DateOnly Date, int TemperatureC, string? Summary) :
        this(default, Date, TemperatureC, Summary, DateTimeOffset.UtcNow) {}
}