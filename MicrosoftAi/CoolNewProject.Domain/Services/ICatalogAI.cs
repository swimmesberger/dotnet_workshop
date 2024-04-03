using CoolNewProject.Domain.Entities;
using Pgvector;

namespace CoolNewProject.Domain.Services;

public interface ICatalogAI {
    bool IsEnabled { get; }

    ValueTask<Vector?> GetEmbeddingAsync(string text);

    ValueTask<Vector?> GetEmbeddingAsync(CatalogItem item);
}
