using CoolNewProject.Domain;
using CoolNewProject.Infrastructure;using CoolNewProject.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(x => {
    x.ReturnHttpNotAcceptable = true;
    x.RespectBrowserAcceptHeader = true;
});
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
app.UseEndpoints(x => x.MapControllers());
if (!app.Environment.IsEnvironment("Testing")) {
    // check and add seed data
    SeedData.Init(app.Services);
}
app.Run();

// required for testing
public partial class Program;