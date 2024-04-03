using CoolNewProject.Domain.Catalog.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using Pgvector;

namespace CoolNewProject.Domain.Catalog;

public sealed class CatalogAi {
    private readonly ITextEmbeddingGenerationService? _embeddingGenerationService;

    /// <summary>Gets whether the AI system is enabled.</summary>
    public bool IsEnabled => _embeddingGenerationService != null;

    /// <summary>Logger for use in AI operations.</summary>
    private readonly ILogger _logger;

    public CatalogAi(ILogger<CatalogAi> logger, ITextEmbeddingGenerationService? embeddingGenerationService = null) {
        _logger = logger;
        _embeddingGenerationService = embeddingGenerationService;
    }


    /// <summary>Gets an embedding vector for the specified text.</summary>
    public async ValueTask<Vector?> GetEmbeddingAsync(string text) {
        if (_embeddingGenerationService == null) {
            return null;
        }

        if (_logger.IsEnabled(LogLevel.Information)) {
            _logger.LogInformation("Getting embedding for \"{Text}\"", text);
        }
        // By default, LocalEmbeddings uses an embeddings model that returns 384-dimensional embedding vectors. Each component is
        // The vector dimension generated here must match with the pgvector definition see CatalogConstants
        // represented by a single-precision float value (4 bytes) = 384*4 = 1536 bytes (in the database per vector)
        var embeddings = await _embeddingGenerationService.GenerateEmbeddingsAsync([text]);
        return new Vector(embeddings[0]);
    }

    /// <summary>Gets an embedding vector for the specified catalog item.</summary>
    public ValueTask<Vector?> GetEmbeddingAsync(CatalogItem item) => IsEnabled
        ? GetEmbeddingAsync($"{item.Name} {item.Description}")
        : ValueTask.FromResult<Vector?>(null);
}
