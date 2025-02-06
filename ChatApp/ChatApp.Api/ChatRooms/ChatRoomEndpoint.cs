using CAP.Infrastructure;
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
        [FromServices] IChatRoomService service,
        CancellationToken cancellationToken = default
    ) {
        var chatRoom = await service.CreateChatRoomAsync(request.Name, cancellationToken);
        return TypedResults.Created($"{this.GetPath()}/{chatRoom.Id}", chatRoom);
    }

    private async Task<ChatRoom?> GetChatRoomById(
        [FromRoute] int id,
        [FromServices] IChatRoomService service,
        CancellationToken cancellationToken = default
    ) => await service.GetChatRoomByIdAsync(id, cancellationToken);

    private async Task<List<ChatRoom>> GetAllChatRooms(
        [FromServices] IChatRoomService service,
        CancellationToken cancellationToken = default
    ) => await service.GetAllChatRoomsAsync(cancellationToken);

    private async Task<List<ChatMessage>> GetAllMessagesByRoomIdAsync(
        [FromRoute] int id,
        [FromServices] IChatRoomService service,
        CancellationToken cancellationToken = default
    ) => await service.GetAllMessagesByRoomIdAsync(id, cancellationToken);

    private async Task<ChatMessage> SendMessage(
        [FromRoute] int id,
        [FromBody] SendMessageRequest request,
        [FromServices] IChatRoomService service,
        CancellationToken cancellationToken = default
    ) => await service.SendMessageAsync(id, request.SenderUserId, request.Content, cancellationToken);

    public sealed record CreateChatRoomRequest {
        public required string Name { get; init; }
    }

    public sealed record SendMessageRequest {
        public required int SenderUserId { get; init; }
        public required string Content { get; init; }
    }
}
