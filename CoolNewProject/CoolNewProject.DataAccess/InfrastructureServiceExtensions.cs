using CoolNewProject.DataAccess.WeatherForecast;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoolNewProject.DataAccess; 

public static class InfrastructureServiceExtensions {
    public static IHostBuilder UseCoolNewProjectInfrastructure(this IHostBuilder builder) {
        builder.UseSqlInfrastructure();
        return builder;
    }
    
    public static IHostBuilder UseSqlInfrastructure(this IHostBuilder builder) {
        builder.ConfigureServices((context, services) => {
            var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<CoolNewProjectDbContext>(options =>
                options.UseSqlServer(connectionString));
        });
        builder.UseWeatherForecast();
        return builder;
    }
}