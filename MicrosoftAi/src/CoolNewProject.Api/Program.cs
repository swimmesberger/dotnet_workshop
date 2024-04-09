using System.Text.Json;
using CoolNewProject.Api.Ai;
using CoolNewProject.Api.Basket;
using CoolNewProject.ServiceDefaults;
using CoolNewProject.Api.Catalog;
using CoolNewProject.Api.Chatbot;
using CoolNewProject.Domain.Catalog.DataAccess;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddDefaultOpenApi();
builder.AddAiServices();
builder.AddCatalogServices();
builder.AddBasketServices();
builder.AddChatbotServices();

builder.Services.AddProblemDetails();

var app = builder.Build();
if (new HashSet<string>(Environment.GetCommandLineArgs()).Contains("--write-mock")) {
    WriteMockData(app);
    return;
}
app.UseDefaultOpenApi();

app.MapDefaultEndpoints();

app.MapGroup("/api/v1/catalog")
    .WithTags("Catalog API")
    .MapCatalogApi();

app.MapGroup("/api/v1/chatbot")
    .WithTags("Chatbot API")
    .MapChatbotApi();

app.MapGroup("/api/v1/basket")
    .WithTags("Basket API")
    .MapBasketApi();

app.Run();
return;


static void WriteMockData(WebApplication app) {
    string contentRootPath = app.Environment.ContentRootPath;
    string destinationPath = Path.Combine(contentRootPath, "Catalog", "Setup", "catalog.json");
    using var scope = app.Services.CreateScope();
    using var dbContext = scope.ServiceProvider.GetRequiredService<CatalogContext>();
    dbContext.Database.Migrate();
    scope.ServiceProvider.GetRequiredService<IDbSeeder<CatalogContext>>().SeedAsync(dbContext).GetAwaiter().GetResult();

    var setupEntries = dbContext
        .CatalogItems
        .Select(x => new CatalogSourceEntry() {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            Brand = x.CatalogBrand.Brand,
            Price = x.Price,
            Type = x.CatalogType.Type,
            Embedding = x.Embedding!.ToArray()
        })
        .ToList();
    using var outputFileStream = File.Open(destinationPath, FileMode.Create);
    JsonSerializer.Serialize(outputFileStream, setupEntries, new JsonSerializerOptions() {
        WriteIndented = true
    });
}
