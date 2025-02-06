using CAP.Infrastructure;
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
        [FromServices] IUserService service,
        CancellationToken cancellationToken = default
    ) {
        var chatRoom = await service.CreateUserAsync(request.Name, cancellationToken);
        return TypedResults.Created($"{this.GetPath()}/{chatRoom.Id}", chatRoom);
    }

    private async Task<User?> GetUserById(
        [FromRoute] int id,
        [FromServices] IUserService service,
        CancellationToken cancellationToken = default
    ) => await service.GetUserByIdAsync(id, cancellationToken);


    private async Task<List<User>> GetAllUsers(
        [FromServices] IUserService service,
        CancellationToken cancellationToken = default
    ) => await service.GetAllUsersAsync(cancellationToken);

    public sealed record CreateUserRequest {
        public required string Name { get; init; }
    }
}
