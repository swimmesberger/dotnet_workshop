using CoolNewProject.ServiceDefaults;
using CoolNewProject.WebApp;
using CoolNewProject.WebApp.Catalog;
using CoolNewProject.WebApp.Chatbot;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.SignalR;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(x => {
        x.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(5);
    });
builder.Services.Configure<HubOptions>(x => {
    x.MaximumReceiveMessageSize = 10_000;
    x.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
});

builder.Services.AddHttpForwarderWithServiceDiscovery();
builder.AddCatalogServices();
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
