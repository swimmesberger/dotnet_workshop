using CoolNewProject.Domain.WeatherForecast;
using CoolNewProject.Domain.WeatherForecast.Services;
using Xunit.Abstractions;

namespace CoolNewProject.UnitTests.WeatherForecast;

public class WeatherForecastServiceTest {
    private readonly ITestOutputHelper _testOutput;

    public WeatherForecastServiceTest(ITestOutputHelper testOutput) {
        _testOutput = testOutput;
    }

    [Fact]
    public async Task TestWeatherForecast() {
        var service = new WeatherForecastService(new InMemoryForecastRepository(new WeatherForecastEntity[] {
            new() {
                Id = new Guid("0224589c-bc34-4761-b8e6-8b95aba85a00"),
                Date = new DateOnly(2010, 01, 01),
                TemperatureC = 12,
                CreatedAt = new DateTimeOffset(2023, 10, 23, 0, 0, 0, TimeSpan.Zero)
            }
        }));
        var forecasts = await service.GetForecasts();
        _testOutput.WriteLine(string.Join(',', forecasts));
        forecasts.Should().HaveCount(1);
    }
}