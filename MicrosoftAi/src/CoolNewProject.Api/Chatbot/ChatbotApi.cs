using CoolNewProject.Domain.Chatbot;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

namespace CoolNewProject.Api.Chatbot;

public static class ChatbotApi {
    public static IEndpointRouteBuilder MapChatbotApi(this IEndpointRouteBuilder app) {
        app.MapPost("/prompt", PromptAsync);
        return app;
    }

    private static async Task<Ok<ChatMessageContent>> PromptAsync(
        [FromServices] ChatbotService chatbotService,
        [FromBody] IEnumerable<ChatMessageContent> messages,
        CancellationToken cancellationToken = default) {
        return TypedResults.Ok(await chatbotService.PromptAsync(messages, cancellationToken));
    }
}
