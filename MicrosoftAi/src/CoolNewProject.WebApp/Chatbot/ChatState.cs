using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace CoolNewProject.WebApp.Chatbot;

public sealed class ChatState {
    private readonly ILogger _logger;
    private readonly ChatbotService _chatbotService;
    private readonly List<ChatMessageContent> _messages;
    public IReadOnlyList<ChatMessageContent> Messages => _messages;

    public ChatState(ILogger<ChatState> logger, ChatbotService chatbotService) {
        _logger = logger;
        _chatbotService = chatbotService;
        _messages = new List<ChatMessageContent>();
    }

    // NOTE: onMessageAdded is here to provide instant feedback about messages
    public async Task AddUserMessageAsync(string userText, Action onMessageAdded) {
        // Store the user's message
        _messages.Add(new ChatMessageContent(AuthorRole.User, userText));
        onMessageAdded();

        // Get and store the AI's response message
        try {
            ChatMessageContent response = await _chatbotService.PromptAsync(Messages);
            if (!string.IsNullOrWhiteSpace(response.Content)) {
                _messages.Add(response);
            }
        } catch (Exception e) {
            if (_logger.IsEnabled(LogLevel.Error)) {
                _logger.LogError(e, "Error getting chat completions");
            }
            _messages.Add(new ChatMessageContent(AuthorRole.Assistant, "My apologies, but I encountered an unexpected error."));
        }
        onMessageAdded();
    }
}
