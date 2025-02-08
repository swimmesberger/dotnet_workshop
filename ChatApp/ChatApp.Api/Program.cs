using ChatApp.Api.Infrastructure;
using ChatApp.Application;

var builder = WebApplication.CreateBuilder(args);
builder.AddWebApi();
builder.Services.AddActorWebServiceScope();
builder.Services.AddApplicationServices();

var app = builder.Build();
app.MapWebApi();
app.Run();
