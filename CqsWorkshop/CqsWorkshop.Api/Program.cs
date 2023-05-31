using CqsWorkshop.Contract;
using CqsWorkshop.Contract.Commands;
using CqsWorkshop.Contract.Queries;
using CqsWorkshop.Infrastructure;
using Mediator;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<JsonOptions>(options => {
    ConfigureJson(options.SerializerOptions);
});
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options => {
    ConfigureJson(options.JsonSerializerOptions);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Services.UseInfrastructure();

app.MapGet("/customer/{id:guid}", async (Guid id, IMediator mediator, 
    CancellationToken cts) => await mediator.Send(new GetCustomerByIdQuery(id), cts)).WithName("GetCustomerById");
app.MapPost("/customer", async (CustomerForCreationDto customer, IMediator mediator, 
    CancellationToken cts) => {
    await mediator.Send(new CreateCustomerCommand(customer), cts);
    return Results.CreatedAtRoute(
        routeName: "GetCustomerById",
        routeValues: new { id = customer.Id  });
});

app.Run();

static void ConfigureJson(JsonSerializerOptions serializerOptions) {
    serializerOptions.Converters.Add(new JsonStringEnumConverter());
}