using CoolNewProject.Domain.WeatherForecast.Contracts;
using CoolNewProject.Domain.WeatherForecast.Repositories;

namespace CoolNewProject.Domain.WeatherForecast.Services; 

public class WeatherForecastService : IWeatherForecastService {
    private readonly IWeatherForecastRepository _repository;
    
    public WeatherForecastService(IWeatherForecastRepository repository) {
        _repository = repository;
    }
    
    public async Task<List<WeatherForecastDto>> GetForecasts(CancellationToken cancellationToken = default) {
        return await _repository.ListAsync(entity => 
            new WeatherForecastDto(entity.Id, entity.Date, entity.TemperatureC, entity.Summary, entity.CreatedAt), cancellationToken);
    }
}