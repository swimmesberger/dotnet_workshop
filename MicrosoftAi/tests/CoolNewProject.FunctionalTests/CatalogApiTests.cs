using CoolNewProject.Core.Ai;
using CoolNewProject.Domain.Catalog;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoolNewProject.FunctionalTests;

public class CatalogApiTests {
    [Fact]
    public async Task TestAiEmbeddings() {
        var embeddingGenerationService = LocalTextEmbeddingKernelExtensions.CreateLocalEmbeddingGenerationService();
        var ai = new CatalogEmbeddingGeneratorService(NullLogger<CatalogEmbeddingGeneratorService>.Instance, embeddingGenerationService);
        var vector = await ai.GetEmbeddingAsync("Test123");
        // vectors should be 1536 in length because that's what we use in the test data and the vector lengths have to match
        vector!.Memory.Length.Should().Be(384);
    }
}
