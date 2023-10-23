using CoolNewProject.Domain.WeatherForecast;
using CoolNewProject.Domain.WeatherForecast.Repositories;

namespace CoolNewProject.UnitTests.WeatherForecast;

internal class InMemoryForecastRepository : InMemoryRepository<WeatherForecastEntity>, IWeatherForecastRepository {
    public InMemoryForecastRepository() { }
    public InMemoryForecastRepository(IEnumerable<WeatherForecastEntity> items) : base(items) { }
}