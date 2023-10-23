using Ardalis.HttpClientTestExtensions;
using CoolNewProject.Domain.WeatherForecast;
using Xunit.Abstractions;

namespace CoolNewProject.IntegrationTests.WeatherForecast; 

public sealed class WeatherForecastControllerTest : IClassFixture<CustomWebApplicationFactory<Program>>, IClassFixture<MssqlDatabase> {
    private readonly ITestOutputHelper _testOutput;
    private readonly HttpClient _client;


    public WeatherForecastControllerTest(ITestOutputHelper testOutput, CustomWebApplicationFactory<Program> factory, MssqlDatabase mssqlDatabase) {
        _testOutput = testOutput;
        // order is important
        factory.ConnectionString = mssqlDatabase.ConnectionString;
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task ReturnsSeedForecasts() {
        var result = await _client.GetAndDeserializeAsync<List<WeatherForecastDto>>("/WeatherForecast", _testOutput);
        result.Should().HaveCount(5);
    }
}