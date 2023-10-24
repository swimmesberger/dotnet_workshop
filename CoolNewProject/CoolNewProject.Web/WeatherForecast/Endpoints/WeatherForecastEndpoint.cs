using CoolNewProject.Domain.WeatherForecast;
using CoolNewProject.Domain.WeatherForecast.Contracts;
using CoolNewProject.Web.MinimalApi;
using Microsoft.AspNetCore.Mvc;
// ReSharper disable once UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global

namespace CoolNewProject.Web.WeatherForecast.Endpoints; 

/// <summary>
/// Every endpoint method can be grouped into one or more logical classes.
/// It is best practice to use as many classes as possible when there are different requirements (read as DI services),
/// to ensure that every endpoint is separately testable with as less dependencies as possible.
/// </summary>
public static class WeatherForecastEndpoint {
    private const string GroupName = "WeatherForecast";
    
    public sealed class Get : IEndpoint {
        // inject singleton services here/prepare "slow" configuration
        public Get() { }

        public void AddRoute(IEndpointRouteBuilder app) {
            app.MapGet("/weatherforecast", HandleAsync)
                .AllowAnonymous()
                .WithTags(GroupName);
        }

        // inject scoped services in method: Services, token, HttpContext
        public async Task<List<WeatherForecastDto>> HandleAsync([FromServices] IWeatherForecastService forecastService,
            CancellationToken cancellationToken) {
            return await forecastService.GetForecasts(cancellationToken);
        }
    }

    public sealed class GetById : IEndpoint {
        public void AddRoute(IEndpointRouteBuilder app) {
            app.MapGet("/weatherforecast/{id:guid}", HandleAsync)
                .AllowAnonymous()
                .WithTags(GroupName);
        }
        
        // inject scoped services in method: Services, token, HttpContext
        public async Task<WeatherForecastDto?> HandleAsync([FromServices] IWeatherForecastService forecastService,
            [FromRoute] Guid id, CancellationToken cancellationToken) {
            return await forecastService.GetForecastById(id, cancellationToken);
        }
    }
}