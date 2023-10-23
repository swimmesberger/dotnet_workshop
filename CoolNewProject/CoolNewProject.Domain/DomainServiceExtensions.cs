using CoolNewProject.Domain.WeatherForecast;
using Microsoft.Extensions.Hosting;

namespace CoolNewProject.Domain; 

public static class DomainServiceExtensions {
    public static IHostBuilder UseCoolNewProjectDomain(this IHostBuilder builder) {
        builder.UseWeatherForecast();
        return builder;
    }
}