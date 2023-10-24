using System.Linq.Expressions;
using CoolNewProject.Domain.WeatherForecast.Contracts;
using CoolNewProject.Domain.WeatherForecast.Repositories;

namespace CoolNewProject.Domain.WeatherForecast.Services; 

public class WeatherForecastService : IWeatherForecastService {
    private static Expression<Func<WeatherForecastEntity, WeatherForecastDto>> MapDto => 
        entity => new WeatherForecastDto(entity.Id, entity.Date, entity.TemperatureC, entity.Summary, entity.CreatedAt);
    private static Expression<Func<WeatherForecastEntity, object?>> DefaultOrder =>
        entity => entity.CreatedAt;
    
    private readonly IWeatherForecastRepository _repository;
    
    public WeatherForecastService(IWeatherForecastRepository repository) {
        _repository = repository;
    }
    
    public async Task<List<WeatherForecastDto>> GetForecasts(CancellationToken cancellationToken = default) {
        return await _repository.ListAsync(MapDto, cancellationToken);
    }

    public async Task<WeatherForecastDto?> GetForecastById(Guid id, CancellationToken cancellationToken = default) {
        return await _repository.FirstOrDefaultAsync(MapDto, DefaultOrder, cancellationToken);
    }
}