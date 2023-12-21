using CoolNewProject.Domain;
using CoolNewProject.DataAccess;
using CoolNewProject.Web;
using CoolNewProject.Web.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpoints();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();

builder.Host.UseCoolNewProjectDomain();
builder.Host.UseCoolNewProjectInfrastructure();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapEndpointProviders();
if (!app.Environment.IsEnvironment("Testing")) {
    // check and add seed data
    SeedData.Init(app.Services);
}
app.Run();

// required for testing
namespace CoolNewProject.Web {
    public partial class Program{}
}