using CoolNewProject.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using Pgvector;

namespace CoolNewProject.Domain.Services;

public sealed class CatalogAI : ICatalogAI {
    private readonly ITextEmbeddingGenerationService? _embeddingGenerationService;

    /// <summary>Gets whether the AI system is enabled.</summary>
    public bool IsEnabled => _embeddingGenerationService != null;

    /// <summary>Logger for use in AI operations.</summary>
    private readonly ILogger _logger;

    public CatalogAI(ILogger<CatalogAI> logger, ITextEmbeddingGenerationService? embeddingGenerationService = null) {
        _logger = logger;
        _embeddingGenerationService = embeddingGenerationService;
    }


    /// <summary>Gets an embedding vector for the specified text.</summary>
    public async ValueTask<Vector?> GetEmbeddingAsync(string text) {
        if (_embeddingGenerationService == null) {
            return null;
        }

        if (_logger.IsEnabled(LogLevel.Information)) {
            _logger.LogInformation("Getting embedding for \"{text}\"", text);
        }
        return new Vector((await _embeddingGenerationService.GenerateEmbeddingsAsync([text]))[0]);
    }

    /// <summary>Gets an embedding vector for the specified catalog item.</summary>
    public ValueTask<Vector?> GetEmbeddingAsync(CatalogItem item) => IsEnabled
        ? GetEmbeddingAsync($"{item.Name} {item.Description}")
        : ValueTask.FromResult<Vector?>(null);
}
