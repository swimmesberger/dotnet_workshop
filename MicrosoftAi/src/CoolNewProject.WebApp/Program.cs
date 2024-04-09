using CoolNewProject.ServiceDefaults;
using CoolNewProject.WebApp;
using CoolNewProject.WebApp.Basket;
using CoolNewProject.WebApp.Catalog;
using CoolNewProject.WebApp.Chatbot;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(new ServiceDefaultsConfiguration {
    // we configure resilience on a per http client basis
    EnableStandardResilience = false
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpForwarderWithServiceDiscovery();
builder.AddCatalogServices();
builder.AddBasketServices();
builder.AddChatbotServices();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAntiforgery();
app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapForwarder("/product-images/{id}", "http://catalog-api", "/api/v1/catalog/items/{id}/pic");

app.Run();
