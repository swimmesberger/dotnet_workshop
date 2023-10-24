namespace CoolNewProject.Domain.WeatherForecast.Contracts; 

public interface IWeatherForecastService {
    Task<List<WeatherForecastDto>> GetForecasts(CancellationToken cancellationToken = default);
    Task<WeatherForecastDto?> GetForecastById(Guid id, CancellationToken cancellationToken = default);
}