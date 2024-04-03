namespace CoolNewProject.WebApp.Chatbot;

public static class ChatbotExtensions {
    public static void AddChatbotServices(this IHostApplicationBuilder builder) {
        // Application services
        builder.Services.AddScoped<ChatState>();

        // HTTP client registrations
        builder.Services.AddHttpClient<ChatbotService>(o => {
            o.BaseAddress = new Uri("http://catalog-api");
            o.Timeout = TimeSpan.FromMinutes(5);
        });
    }
}
