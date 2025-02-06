using ChatApp.Api.Infrastructure;
using ChatApp.Application;

var builder = WebApplication.CreateBuilder(args);
builder.AddWebApi();
builder.Services.AddApplicationServices();

var app = builder.Build();
app.UseWebServiceScope();
app.MapWebApi();
app.Run();
