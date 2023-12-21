using CoolNewProject.Domain.WeatherForecast;
using CoolNewProject.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace CoolNewProject.Web; 

public sealed class SeedData {
    private readonly string[] _summaries = {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly Random _random;

    public SeedData() : this(Random.Shared) { }

    public SeedData(Random random) {
        _random = random;
    }

    public void Initialize(IServiceProvider serviceProvider) {
        using var scope = serviceProvider.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<CoolNewProjectDbContext>();
        if (dbContext.Database.IsRelational()) {
            dbContext.Database.Migrate();
        } else {
            dbContext.Database.EnsureCreated();
        }
        // Look for any forecasts
        if (dbContext.WeatherForecasts.Any()) {
            return; // DB has been seeded
        }
        PopulateTestData(dbContext);
    }

    private void PopulateTestData(CoolNewProjectDbContext dbContext) {
        foreach (var item in dbContext.WeatherForecasts) {
            dbContext.Remove(item);
        }
        dbContext.SaveChanges();
        
        foreach (WeatherForecastEntity weatherForecast in Enumerable.Range(1, 5)
                     .Select(index =>
                         new WeatherForecastEntity {
                             Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                             TemperatureC = _random.Next(-20, 55),
                             Summary = _summaries[_random.Next(_summaries.Length)]
                         })) {
            dbContext.WeatherForecasts.Add(weatherForecast);
        }

        dbContext.SaveChanges();
    }

    public static void Init(IServiceProvider serviceProvider) {
        new SeedData().Initialize(serviceProvider);
    }
}