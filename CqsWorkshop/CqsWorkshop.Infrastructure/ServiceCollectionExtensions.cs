using CqsWorkshop.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CqsWorkshop.Infrastructure; 

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) {
        services.AddDbContext<OrderManagementDbContext>(options => 
            options.UseSqlServer(configuration.GetConnectionString("OrderDatabase")));
        services.AddMediator();
        return services;
    }
    
    public static IServiceProvider UseInfrastructure(this IServiceProvider sp) {
        using var scope = sp.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
        dbContext.Database.Migrate();
        return sp;
    }
}