using CoolNewProject.DataAccess;
using CoolNewProject.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoolNewProject.IntegrationTests;

public sealed class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class {
    public string ConnectionString { get; set; } = "";
    public Random Seed { get; set; } = new Random(42);
    
    protected override IHost CreateHost(IHostBuilder builder) {
        builder.UseEnvironment("Testing");
        
        var host = builder.Build();
        host.Start();

        // Get service provider.
        var serviceProvider = host.Services;
        
        // check and add seed data
        new SeedData(Seed).Initialize(serviceProvider);

        return host;
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.ConfigureServices(services => {
                // Remove the app's ApplicationDbContext registration.
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                         typeof(DbContextOptions<CoolNewProjectDbContext>));

                if (descriptor != null) {
                    services.Remove(descriptor);
                }
                // Add ApplicationDbContext using the container database for testing
                services.AddDbContext<CoolNewProjectDbContext>(options => {
                    options.UseSqlServer(ConnectionString);
                });
            });
    }
}