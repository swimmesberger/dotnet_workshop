using CqsWorkshop.Contract;
using CqsWorkshop.Contract.Commands;
using CqsWorkshop.Contract.Queries;
using Mediator;

namespace CqsWorkshop.Api; 

public static class CustomerApiExtensions {
    public static IEndpointRouteBuilder UseCustomerApi(this IEndpointRouteBuilder app) {
        app.MapGet("/customer/{id:guid}", async (Guid id, IMediator mediator, 
            CancellationToken cts) => await mediator.Send(new GetCustomerByIdQuery(id), cts)).WithName("GetCustomerById");
        app.MapPost("/customer", async (CustomerForCreationDto customer, IMediator mediator, 
            CancellationToken cts) => {
            customer = customer with{ Id = Guid.NewGuid() };
            await mediator.Send(new CreateCustomerCommand(customer), cts);
            //var customerRe = await mediator.Send(new GetCustomerByIdQuery(customer.Id));
            return Results.CreatedAtRoute(
                routeName: "GetCustomerById",
                routeValues: new { id = customer.Id  },
                value: default);
        });
        return app;
    }
}