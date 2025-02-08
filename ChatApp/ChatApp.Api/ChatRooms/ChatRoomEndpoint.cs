using ChatApp.Api.Infrastructure;
using ChatApp.Application.ChatRooms;
using ChatApp.Application.Domain.ChatRooms;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.ChatRooms;

public sealed class ChatRoomEndpoint : EndpointGroupBase {
    public override void Map(IEndpointRouteBuilder routeBuilder) {
        routeBuilder.MapGet("/", GetAllChatRooms).WithDefaultMetadata();
        routeBuilder.MapGet("/{id:int}", GetChatRoomById).WithDefaultMetadata();
        routeBuilder.MapGet("/{id:int}/message", GetAllMessagesByRoomIdAsync).WithDefaultMetadata();

        routeBuilder.MapPost("/", CreateChatRoom).WithDefaultMetadata();
        routeBuilder.MapPost("/{id:int}/user", JoinChatRoom).WithDefaultMetadata();
        routeBuilder.MapPost("/{id:int}/message", SendMessage).WithDefaultMetadata();
    }

    private async Task<Created<ChatRoom>> CreateChatRoom(
        [FromBody] CreateChatRoomRequest request,
        [FromServices] ChatRoomClient client,
        HttpContext context,
        CancellationToken cancellationToken = default
    ) {
        var chatRoom = await client.CreateChatRoomAsync(request.Name, context.ToClientRequestOptions(), cancellationToken);
        return TypedResults.Created($"{this.GetPath()}/{chatRoom.Id}", chatRoom);
    }

    private async Task JoinChatRoom(
        [FromRoute] int id,
        [FromBody] JoinChatRoomRequest request,
        [FromServices] ChatRoomClient client,
        HttpContext context,
        CancellationToken cancellationToken = default
    ) {
        await client.JoinChatRoomAsync(id, request.JoinUserId, context.ToClientRequestOptions(), cancellationToken);
    }

    private async Task<ChatRoom?> GetChatRoomById(
        [FromRoute] int id,
        [FromServices] ChatRoomClient client,
        HttpContext context,
        CancellationToken cancellationToken = default
    ) => await client.GetChatRoomByIdAsync(id, context.ToClientRequestOptions(), cancellationToken);

    private async Task<List<ChatRoom>> GetAllChatRooms(
        [FromServices] ChatRoomClient client,
        HttpContext context,
        CancellationToken cancellationToken = default
    ) => await client.GetAllChatRoomsAsync(context.ToClientRequestOptions(), cancellationToken);

    private async Task<List<ChatMessage>> GetAllMessagesByRoomIdAsync(
        [FromRoute] int id,
        [FromServices] ChatRoomClient client,
        HttpContext context,
        CancellationToken cancellationToken = default
    ) => await client.GetAllMessagesByRoomIdAsync(id, context.ToClientRequestOptions(), cancellationToken);

    private async Task<ChatMessage> SendMessage(
        [FromRoute] int id,
        [FromBody] SendMessageRequest request,
        [FromServices] ChatRoomClient client,
        HttpContext context,
        CancellationToken cancellationToken = default
    ) => await client.SendMessageAsync(id, request.SenderUserId, request.Content, context.ToClientRequestOptions(), cancellationToken);

    public sealed record CreateChatRoomRequest {
        public required string Name { get; init; }
    }

    public sealed record JoinChatRoomRequest {
        public required int JoinUserId { get; init; }
    }

    public sealed record SendMessageRequest {
        public required int SenderUserId { get; init; }
        public required string Content { get; init; }
    }
}
