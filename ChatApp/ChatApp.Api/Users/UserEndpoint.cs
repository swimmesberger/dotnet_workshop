using ChatApp.Api.Infrastructure;
using ChatApp.Application;
using ChatApp.Application.Users;
using ChatApp.Domain.Users;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Users;

public sealed class UserEndpoint : EndpointGroupBase {
    public override void Map(IEndpointRouteBuilder routeBuilder) {
        routeBuilder.MapGet("/", GetAllUsers).WithDefaultMetadata();
        routeBuilder.MapGet("/{id:int}", GetUserById).WithDefaultMetadata();

        routeBuilder.MapPost("/", CreateUser).WithDefaultMetadata();
    }

    public async Task<Created<User>> CreateUser(
        [FromBody] CreateUserRequest request,
        [FromServices] UserClient client,
        HttpContext httpContext,
        CancellationToken cancellationToken = default
    ) {
        var chatRoom = await client.CreateUserAsync(request.Name, new ClientRequestOptions {
          RequestId  = httpContext.TraceIdentifier
        }, cancellationToken);
        return TypedResults.Created($"{this.GetPath()}/{chatRoom.Id}", chatRoom);
    }

    private async Task<User?> GetUserById(
        [FromRoute] int id,
        [FromServices] UserClient client,
        HttpContext httpContext,
        CancellationToken cancellationToken = default
    ) => await client.GetUserByIdAsync(id, new ClientRequestOptions {
        RequestId  = httpContext.TraceIdentifier
    }, cancellationToken);


    private async Task<List<User>> GetAllUsers(
        [FromServices] UserClient client,
        HttpContext httpContext,
        CancellationToken cancellationToken = default
    ) => await client.GetAllUsersAsync(new ClientRequestOptions {
        RequestId  = httpContext.TraceIdentifier
    }, cancellationToken);

    public sealed record CreateUserRequest {
        public required string Name { get; init; }
    }
}
