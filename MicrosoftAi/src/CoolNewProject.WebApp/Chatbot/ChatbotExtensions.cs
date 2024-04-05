using Microsoft.Extensions.Http.Resilience;

namespace CoolNewProject.WebApp.Chatbot;

public static class ChatbotExtensions {
    public static void AddChatbotServices(this IHostApplicationBuilder builder) {
        // Application services
        builder.Services.AddScoped<ChatState>();

        // HTTP client registrations
        var resilienceBuilder = builder.Services.AddHttpClient<ChatbotService>(o =>
            o.BaseAddress = new Uri("http://catalog-api"))
            .AddStandardResilienceHandler();
        var resilienceOptions = builder.Configuration.GetSection("HttpClientResilience:Chatbot");
        if (resilienceOptions.GetChildren().Any()) {
            resilienceBuilder.Configure(resilienceOptions);
        }
    }
}
