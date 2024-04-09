using CoolNewProject.Domain.Catalog.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using Pgvector;

namespace CoolNewProject.Domain.Catalog;

public sealed class EmbeddingGenerator {
    private readonly ITextEmbeddingGenerationService _embeddingGenerationService;

    public EmbeddingGenerator(ITextEmbeddingGenerationService embeddingGenerationService) {
        _embeddingGenerationService = embeddingGenerationService;
    }

    public async ValueTask<Vector?> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default) {
        var embeddings = await _embeddingGenerationService.GenerateEmbeddingsAsync([text], kernel: null, cancellationToken);
        return new Vector(embeddings[0]);
    }
}
