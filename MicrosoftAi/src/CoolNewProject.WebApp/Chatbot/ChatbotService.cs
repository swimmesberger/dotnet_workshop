using Microsoft.SemanticKernel;

namespace CoolNewProject.WebApp.Chatbot;

public sealed class ChatbotService(HttpClient httpClient) {
    private const string RemoteServiceBaseUrl = "api/v1/chatbot/";

    public async Task<ChatMessageContent> PromptAsync(IEnumerable<ChatMessageContent> messages, CancellationToken cancellationToken = default) {
        string uri = $"{RemoteServiceBaseUrl}prompt";
        var result = await httpClient.PostAsJsonAsync(uri, messages, cancellationToken: cancellationToken);
        return (await result.Content.ReadFromJsonAsync<ChatMessageContent>(cancellationToken: cancellationToken))!;
    }
}
