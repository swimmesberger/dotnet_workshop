using ChatApp.Api.Infrastructure;
using ChatApp.Application;
using ChatApp.Application.ChatRooms;
using ChatApp.Domain.ChatRooms;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.ChatRooms;

public sealed class ChatRoomEndpoint : EndpointGroupBase {
    public override void Map(IEndpointRouteBuilder routeBuilder) {
        routeBuilder.MapGet("/", GetAllChatRooms).WithDefaultMetadata();
        routeBuilder.MapGet("/{id:int}", GetChatRoomById).WithDefaultMetadata();
        routeBuilder.MapGet("/{id:int}/message", GetAllMessagesByRoomIdAsync).WithDefaultMetadata();

        routeBuilder.MapPost("/", CreateChatRoom).WithDefaultMetadata();
        routeBuilder.MapPost("/{id:int}/message", SendMessage).WithDefaultMetadata();
    }

    public async Task<Created<ChatRoom>> CreateChatRoom(
        [FromBody] CreateChatRoomRequest request,
        [FromServices] ChatRoomClient client,
        HttpContext context,
        CancellationToken cancellationToken = default
    ) {
        var chatRoom = await client.CreateChatRoomAsync(request.Name, new ClientRequestOptions {
            RequestId = context.TraceIdentifier
        }, cancellationToken);
        return TypedResults.Created($"{this.GetPath()}/{chatRoom.Id}", chatRoom);
    }

    private async Task<ChatRoom?> GetChatRoomById(
        [FromRoute] int id,
        [FromServices] ChatRoomClient client,
        HttpContext context,
        CancellationToken cancellationToken = default
    ) => await client.GetChatRoomByIdAsync(id, new ClientRequestOptions {
        RequestId = context.TraceIdentifier
    }, cancellationToken);

    private async Task<List<ChatRoom>> GetAllChatRooms(
        [FromServices] ChatRoomClient client,
        HttpContext context,
        CancellationToken cancellationToken = default
    ) => await client.GetAllChatRoomsAsync(new ClientRequestOptions {
        RequestId = context.TraceIdentifier
    },cancellationToken);

    private async Task<List<ChatMessage>> GetAllMessagesByRoomIdAsync(
        [FromRoute] int id,
        [FromServices] ChatRoomClient client,
        HttpContext context,
        CancellationToken cancellationToken = default
    ) => await client.GetAllMessagesByRoomIdAsync(id, new ClientRequestOptions {
        RequestId = context.TraceIdentifier
    }, cancellationToken);

    private async Task<ChatMessage> SendMessage(
        [FromRoute] int id,
        [FromBody] SendMessageRequest request,
        [FromServices] ChatRoomClient client,
        HttpContext context,
        CancellationToken cancellationToken = default
    ) => await client.SendMessageAsync(id, request.SenderUserId, request.Content, new ClientRequestOptions {
        RequestId = context.TraceIdentifier
    }, cancellationToken);

    public sealed record CreateChatRoomRequest {
        public required string Name { get; init; }
    }

    public sealed record SendMessageRequest {
        public required int SenderUserId { get; init; }
        public required string Content { get; init; }
    }
}
