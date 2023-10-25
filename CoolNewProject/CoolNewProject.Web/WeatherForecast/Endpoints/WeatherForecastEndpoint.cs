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
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class WeatherForecastEndpoint : IEndpointProvider {
    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder app) {
        var root = app.MapGroup("/weatherforecast")
            .WithTags("WeatherForecast")
            .WithOpenApi()
            .AllowAnonymous();
        root.MapGet<Get>("/");
        root.MapGet<GetById>("/{id:guid}");
        return app;
    }
    
    public sealed class Get : IEndpoint {
        private readonly IWeatherForecastService _forecastService;

        public Get(IWeatherForecastService forecastService) {
            _forecastService = forecastService;
        }
        
        public async Task<List<WeatherForecastDto>> HandleAsync(CancellationToken cancellationToken) {
            return await _forecastService.GetForecasts(cancellationToken);
        }
    }

    public sealed class GetById : IEndpoint {
        private readonly IWeatherForecastService _forecastService;

        public GetById(IWeatherForecastService forecastService) {
            _forecastService = forecastService;
        }
        
        public async Task<IResult> HandleAsync([FromRoute] Guid id, CancellationToken cancellationToken) {
            return Results.Ok(await _forecastService.GetForecastById(id, cancellationToken));
        }
    }
}