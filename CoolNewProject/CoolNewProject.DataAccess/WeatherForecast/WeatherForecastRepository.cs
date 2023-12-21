using Ardalis.Specification;
using CoolNewProject.Domain.WeatherForecast;
using CoolNewProject.Domain.WeatherForecast.Repositories;

namespace CoolNewProject.DataAccess.WeatherForecast; 

public sealed class WeatherForecastRepository : EfBaseRepository<WeatherForecastEntity>, IWeatherForecastRepository {
    public WeatherForecastRepository(CoolNewProjectDbContext dbContext, ISpecificationEvaluator specificationEvaluator) : 
        base(dbContext, specificationEvaluator) {
    }

    public WeatherForecastRepository(CoolNewProjectDbContext dbContext) : base(dbContext) { }
}