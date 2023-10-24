using Ardalis.HttpClientTestExtensions;
using CoolNewProject.Domain.WeatherForecast;
using CoolNewProject.Web;
using Xunit.Abstractions;

namespace CoolNewProject.UnitTests.WeatherForecast; 

public sealed class WeatherForecastControllerTest : IClassFixture<CustomWebApplicationFactory<Program>> {
    private readonly ITestOutputHelper _testOutput;
    private readonly HttpClient _client;


    public WeatherForecastControllerTest(ITestOutputHelper testOutput, CustomWebApplicationFactory<Program> factory) {
        _testOutput = testOutput;
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task ReturnsSeedForecasts() {
        var result = await _client.GetAndDeserializeAsync<List<WeatherForecastDto>>("/WeatherForecast", _testOutput);
        result.Should().HaveCount(5);
    }
}