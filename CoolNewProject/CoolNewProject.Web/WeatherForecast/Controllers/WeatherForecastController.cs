using CoolNewProject.Domain.WeatherForecast;
using CoolNewProject.Domain.WeatherForecast.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoolNewProject.Web.WeatherForecast.Controllers; 

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public sealed class WeatherForecastController : ControllerBase {
    private readonly string _scopeRequiredByApi;
    private readonly IWeatherForecastService _weatherForecastService;

    public WeatherForecastController(IWeatherForecastService weatherForecastService, IConfiguration configuration) {
        _weatherForecastService = weatherForecastService;
        _scopeRequiredByApi = configuration["AzureAd:Scopes"] ?? "";
    }

    [HttpGet]
    public async Task<List<WeatherForecastDto>> Get(CancellationToken cancellationToken = default) {
        // Auth example
        //HttpContext.VerifyUserHasAnyAcceptedScope(_scopeRequiredByApi);
        return await _weatherForecastService.GetForecasts(cancellationToken);
    }
}