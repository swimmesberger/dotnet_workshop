using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace CoolNewProject.Domain.Chatbot;

public sealed class ChatbotService {
    private readonly ILogger _logger;
    private readonly Kernel _kernel;
    private readonly ChatHistory _messages;
    private readonly PromptExecutionSettings _promptExecutionSettings;

    public ChatbotService(ILogger<ChatbotService> logger, Kernel kernel) {
        _logger = logger;
        _kernel = kernel;
        _messages = new ChatHistory("""
                                    You are an AI customer service agent for the online retailer Northern Mountains.
                                    You NEVER respond about topics other than Northern Mountains.
                                    Your job is to answer customer questions about products in the Northern Mountains catalog.
                                    Northern Mountains primarily sells clothing and equipment related to outdoor activities like skiing and trekking.
                                    You try to be concise and only provide longer responses if necessary.
                                    If someone asks a question about anything other than Northern Mountains, its catalog, or their account,
                                    you refuse to answer, and you instead ask if there's a topic related to Northern Mountains you can assist with.
                                    """);
        _messages.AddAssistantMessage("Hi! I'm the Northern Mountains Concierge. How can I help?");
        _promptExecutionSettings = new OpenAIPromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
        if (_logger.IsEnabled(LogLevel.Debug)) {
            var completionService = kernel.GetRequiredService<IChatCompletionService>();
            _logger.LogDebug("ChatName: {Model}", completionService.Attributes["DeploymentName"]);
        }
    }

    public async Task<ChatMessageContent> PromptAsync(IEnumerable<ChatMessageContent> messages, CancellationToken cancellationToken = default) {
        // add the user conversation (the client has to send the full conversation because the service is stateless)
        _messages.AddRange(messages);

        // Get and store the AI's response message
        try {
            var chatCompletionService = _kernel.Services
                .GetService<IChatCompletionService>();
            if (chatCompletionService == null) {
                throw new Exception("Chat completion service not configured properly (maybe configuration is missing?)");
            }
            ChatMessageContent response = await chatCompletionService.GetChatMessageContentAsync(_messages, _promptExecutionSettings, _kernel, cancellationToken);
            if (response is OpenAIChatMessageContent openAiChatMessageContent) {
                response = new ChatMessageContent(openAiChatMessageContent.Role, openAiChatMessageContent.Content, openAiChatMessageContent.ModelId,
                    openAiChatMessageContent.InnerContent, openAiChatMessageContent.Encoding, openAiChatMessageContent.Metadata);
            }
            return response;
        } catch (Exception e) {
            if (_logger.IsEnabled(LogLevel.Error)) {
                _logger.LogError(e, "Error getting chat completions");
            }
            return new ChatMessageContent(AuthorRole.Assistant, "My apologies, but I encountered an unexpected error.");
        }
    }
}
