using CoolNewProject.Domain.Catalog.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using Pgvector;
#pragma warning disable SKEXP0001

namespace CoolNewProject.Domain.Catalog;

public sealed class CatalogEmbeddingGeneratorService {
    private readonly ITextEmbeddingGenerationService? _embeddingGenerationService;

    /// <summary>Gets whether the AI system is enabled.</summary>
    public bool IsEnabled => _embeddingGenerationService != null;

    /// <summary>Logger for use in AI operations.</summary>
    private readonly ILogger _logger;

    public CatalogEmbeddingGeneratorService(ILogger<CatalogEmbeddingGeneratorService> logger, ITextEmbeddingGenerationService? embeddingGenerationService = null) {
        _logger = logger;
        _embeddingGenerationService = embeddingGenerationService;
    }


    /// <summary>Gets an embedding vector for the specified text.</summary>
    public async ValueTask<Vector?> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default) {
        if (_embeddingGenerationService == null) {
            return null;
        }

        if (_logger.IsEnabled(LogLevel.Information)) {
            _logger.LogInformation("Getting embedding for \"{Text}\"", text);
        }
        // NOTE: kernel parameter not used internally for local embedder - change if other service is used
        var embeddings = await _embeddingGenerationService.GenerateEmbeddingsAsync([text], kernel: null, cancellationToken);
        return new Vector(embeddings[0]);
    }

    /// <summary>Gets an embedding vector for the specified catalog item.</summary>
    public ValueTask<Vector?> GetEmbeddingAsync(CatalogItem item, CancellationToken cancellationToken = default) => IsEnabled
        ? GetEmbeddingAsync($"{item.Name} {item.Description}", cancellationToken)
        : ValueTask.FromResult<Vector?>(null);
}
