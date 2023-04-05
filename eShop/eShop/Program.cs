using System.Text.Json.Serialization;
using eShop.BackgroundServices;
using eShop.Infrastructure;
using eShop.Jobs;
using eShop.Scheduler;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JsonOptions>(options => {
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// register job scheduler
builder.Services.AddSingleton<DefaultJobScheduler>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DefaultJobScheduler>());
builder.Services.AddSingleton<IJobScheduler>(sp => sp.GetRequiredService<DefaultJobScheduler>());
// register expired cart cleanup service
builder.Services.AddHostedService<CartCleanupService>();
builder.Services.AddDbContext<ShopDbContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("Main"));
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapPost("/import", (IFormFile importFile, IJobScheduler jobScheduler) => {
    using var memoryStream = new MemoryStream();
    importFile.CopyTo(memoryStream);
    var importData = memoryStream.ToArray();
    var job = new ImportJob(importData);
    _ = jobScheduler.EnqueueJob(job);
    return job.Id;
});
app.MapGet("/import", (IJobScheduler jobScheduler) => jobScheduler.GetJobs<ImportJob>());
app.MapGet("/import/{id:guid}", (Guid id, IJobScheduler jobScheduler) => jobScheduler.GetJobOrDefault<ImportJob>(id));
using (var scope = app.Services.CreateScope()) {
    scope.ServiceProvider
        .GetRequiredService<ShopDbContext>()
        .Database
        .Migrate();
}
app.Run();