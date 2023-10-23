using CoolNewProject.Domain.WeatherForecast.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoolNewProject.Infrastructure.WeatherForecast; 

internal static class WeatherForecastServiceExtensions {
    public static IHostBuilder UseWeatherForecast(this IHostBuilder builder) {
        builder.ConfigureServices((_, services) => {
            services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
        });
        return builder;
    }
}