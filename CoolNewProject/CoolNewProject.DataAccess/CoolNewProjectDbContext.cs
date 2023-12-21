using CoolNewProject.Domain.WeatherForecast;
using Microsoft.EntityFrameworkCore;

namespace CoolNewProject.DataAccess; 

public sealed class CoolNewProjectDbContext : DbContext {
    public DbSet<WeatherForecastEntity> WeatherForecasts => Set<WeatherForecastEntity>();
    
    public CoolNewProjectDbContext(DbContextOptions<CoolNewProjectDbContext> options) : base(options) {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoolNewProjectDbContext).Assembly);
    }
}