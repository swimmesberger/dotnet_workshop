using System.Data.Common;
using Aspire.Azure.AI.OpenAI;
using CoolNewProject.Domain.Chatbot;
using Microsoft.SemanticKernel;

namespace CoolNewProject.Api.Ai;

public static class AiExtensions {
    private const string ConnectionStringEndpoint = "Endpoint";
    private const string ConnectionStringKey = "Key";

    public static void AddAiServices(this IHostApplicationBuilder builder) {
        var kernelBuilder = builder.Services.AddKernel();
        // The default model that gets downloaded on build is bge-micro-v2, an MIT-licensed BERT embedding model,
        // which has been quantized down to just 22.9 MiB, runs efficiently on CPU, and scores well on benchmarks -
        // outperforming many gigabyte-sized models.
        // By default, LocalEmbeddings uses an embeddings model that returns 384-dimensional embedding vectors.
        // The vector dimension generated here must match with the pgvector definition see CatalogConstants
        // represented by a single-precision float value (4 bytes) = 384*4 = 1536 bytes (in the database per vector)
        kernelBuilder.AddLocalTextEmbeddingGeneration();

        // Add OpenAI chat completion service
        var openAiConnectionString = builder.Configuration.GetConnectionString("openai");
        if (!string.IsNullOrWhiteSpace(openAiConnectionString)) {
            // AddAzureOpenAIChatCompletion resolves this client
            builder.Services.AddSingleton(new HttpClient {
                Timeout = TimeSpan.FromMinutes(5)
            });

            var azureOpenAiSettings = ParseOpenAiConnectionString(openAiConnectionString);
            kernelBuilder.AddAzureOpenAIChatCompletion(
                deploymentName: "gpt-4",
                endpoint: azureOpenAiSettings.Endpoint?.ToString() ?? throw new InvalidOperationException(),
                apiKey: azureOpenAiSettings.Key ?? throw new InvalidOperationException()
            );
        }
    }

    private static AzureOpenAISettings ParseOpenAiConnectionString(string connectionString) {
        var connectionBuilder = new DbConnectionStringBuilder {
            ConnectionString = connectionString
        };
        var azureOpenAiSettings = new AzureOpenAISettings();
        if (connectionBuilder.ContainsKey(ConnectionStringEndpoint) &&
            Uri.TryCreate(connectionBuilder[ConnectionStringEndpoint].ToString(), UriKind.Absolute, out var serviceUri)) {
            azureOpenAiSettings.Endpoint = serviceUri;
        } else if (Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)) {
            azureOpenAiSettings.Endpoint = uri;
        }
        if (connectionBuilder.ContainsKey(ConnectionStringKey)) {
            azureOpenAiSettings.Key = connectionBuilder[ConnectionStringKey].ToString();
        }
        return azureOpenAiSettings;
    }
}
