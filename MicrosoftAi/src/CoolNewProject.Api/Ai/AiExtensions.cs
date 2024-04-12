using System.Data.Common;
using CoolNewProject.Core.Ai;
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
            // CAUTION: with this definition the OpenAI api client does not use standard resilience
            //          when the http client is not defined here the standard resilience timeout if used which is not enough
            //          haven't found a way yet to use standard resilience with custom timeouts
            var httpClient = new HttpClient() {
                Timeout = TimeSpan.FromMinutes(5)
            };
            // AddAzureOpenAIChatCompletion resolves this client
            var azureOpenAiSettings = ParseOpenAiConnectionString(openAiConnectionString);
            kernelBuilder.AddAzureOpenAIChatCompletion(
                deploymentName: "gpt-4",
                endpoint: azureOpenAiSettings.Endpoint.ToString(),
                apiKey: azureOpenAiSettings.Key,
                httpClient: httpClient
            );
        }
    }

    private static AzureOpenAISettings ParseOpenAiConnectionString(string connectionString) {
        var connectionBuilder = new DbConnectionStringBuilder {
            ConnectionString = connectionString
        };
        Uri? endpoint = null;
        string key = string.Empty;
        if (connectionBuilder.ContainsKey(ConnectionStringEndpoint) &&
            Uri.TryCreate(connectionBuilder[ConnectionStringEndpoint].ToString(), UriKind.Absolute, out var serviceUri)) {
            endpoint = serviceUri;
        } else if (Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)) {
            endpoint = uri;
        }
        if (connectionBuilder.ContainsKey(ConnectionStringKey)) {
            key = connectionBuilder[ConnectionStringKey].ToString() ?? string.Empty;
        }

        if (endpoint == null || string.IsNullOrEmpty(key))
            throw new ArgumentException("Invalid azure OpenAI connection string");
        return new AzureOpenAISettings(endpoint, key);
    }

    private sealed record AzureOpenAISettings(Uri Endpoint, string Key);
}
