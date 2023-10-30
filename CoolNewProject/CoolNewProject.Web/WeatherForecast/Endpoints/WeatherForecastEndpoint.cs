using System.Diagnostics.CodeAnalysis;
using CoolNewProject.Domain.WeatherForecast;
using CoolNewProject.Domain.WeatherForecast.Contracts;
using CoolNewProject.Web.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace CoolNewProject.Web.WeatherForecast.Endpoints; 

/// <summary>
/// Every endpoint method can be grouped into one or more logical classes.
/// It is best practice to use as many classes as possible when there are different requirements (read as DI services),
/// to ensure that every endpoint is separately testable with as less dependencies as possible.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed class WeatherForecastEndpoint : IEndpointProvider {
    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder app) {
        var root = app.MapGroup("/weatherforecast")
            .WithTags("WeatherForecast")
            .WithOpenApi()
            .AllowAnonymous();
        root.MapGet("/", HandleGetAsync);
        root.MapGet("/{id:guid}", HandleGetByIdAsync);
        return app;
    }
    
    private async Task<List<WeatherForecastDto>> HandleGetAsync(
        [FromServices] IWeatherForecastService forecastService, 
        CancellationToken cancellationToken
    ) => await forecastService.GetForecasts(cancellationToken);

    private async Task<IResult> HandleGetByIdAsync(
        [FromServices] IWeatherForecastService forecastService, 
        [FromRoute] Guid id, 
        CancellationToken cancellationToken
    ) => Results.Ok(await forecastService.GetForecastById(id, cancellationToken));
}