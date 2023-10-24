using CoolNewProject.Domain.WeatherForecast;
using CoolNewProject.Domain.WeatherForecast.Contracts;
using CoolNewProject.Web.Endpoints;
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
public sealed class WeatherForecastEndpoint : IEndpointProvider {
    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder app) {
        var root = app.MapGroup("/weatherforecast")
            .WithTags("WeatherForecast")
            .WithOpenApi()
            .AllowAnonymous();
        // register all nested types of this type
        root.MapEndpoints<WeatherForecastEndpoint>();
        // OR register every endpoint explicitly
        //root.MapEndpoint<Get>();
        //root.MapEndpoint<GetById>();
        return app;
    }

    public sealed class Get : IEndpoint {
        // inject singleton services here/prepare "slow" configuration
        public Get() { }

        public void AddRoute(IEndpointRouteBuilder app) {
            app.MapGet("/", HandleAsync);
        }

        // inject scoped services in method: Services, token, HttpContext
        public async Task<List<WeatherForecastDto>> HandleAsync([FromServices] IWeatherForecastService forecastService,
            CancellationToken cancellationToken) {
            return await forecastService.GetForecasts(cancellationToken);
        }
    }

    public sealed class GetById : IEndpoint {
        public void AddRoute(IEndpointRouteBuilder app) {
            app.MapGet("/{id:guid}", HandleAsync);
        }
        
        // inject scoped services in method: Services, token, HttpContext
        public async Task<IResult> HandleAsync([FromServices] IWeatherForecastService forecastService,
            [FromRoute] Guid id, CancellationToken cancellationToken) {
            return Results.Ok(await forecastService.GetForecastById(id, cancellationToken));
        }
    }
}