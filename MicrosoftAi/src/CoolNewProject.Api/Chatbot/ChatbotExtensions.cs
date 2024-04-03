using CoolNewProject.Domain.Chatbot;
using Microsoft.SemanticKernel;

namespace CoolNewProject.Api.Chatbot;

public static class ChatbotExtensions {
    public static void AddChatbotServices(this IHostApplicationBuilder builder) {
        builder.Services.AddScoped<ChatbotService>();
        builder.Services.AddTransient<KernelPlugin>(sp =>
            KernelPluginFactory.CreateFromType<ChatbotInteractions>(serviceProvider: sp));
    }
}
