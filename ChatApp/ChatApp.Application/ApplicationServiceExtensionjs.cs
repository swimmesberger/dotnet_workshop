using ChatApp.Application.ChatRooms;
using ChatApp.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.Application;

public static class ApplicationServiceExtensions {
    public static IServiceCollection AddApplicationServices(this IServiceCollection services) {
        services
            .AddActorSystem()
            .MapActor<ChatRoomManagementActor>()
            .MapActor<UserManagementActor>();
        services.AddScoped<ChatRoomService>();
        services.AddScoped<ChatRoomClient>();
        services.AddScoped<ChatRoomManagementService>();
        services.AddScoped<UserManagementService>();
        services.AddScoped<UserClient>();
        return services;
    }
}
