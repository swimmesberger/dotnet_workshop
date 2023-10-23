using CoolNewProject.Domain.WeatherForecast.Contracts;
using CoolNewProject.Domain.WeatherForecast.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoolNewProject.Domain.WeatherForecast; 

internal static class WeatherForecastServiceExtensions {
    public static IHostBuilder UseWeatherForecast(this IHostBuilder builder) {
        builder.ConfigureServices((_, services) => {
            services.AddScoped<IWeatherForecastService, WeatherForecastService>();
        });
        return builder;
    }
}