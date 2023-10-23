using CoolNewProject.Domain.WeatherForecast;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoolNewProject.Infrastructure.WeatherForecast; 

public sealed class WeatherForecastConfiguration : IEntityTypeConfiguration<WeatherForecastEntity>  {
    public void Configure(EntityTypeBuilder<WeatherForecastEntity> builder) {
        builder.HasKey(e => e.Id).IsClustered(false);
        builder.HasIndex(e => e.CreatedAt).IsClustered();
        builder.Property(e => e.Date).HasConversion<DateOnlyConverter, DateOnlyComparer>();
        builder.Property(e => e.Summary).HasMaxLength(100);
        builder.Property(e => e.TemperatureC);
    }
}