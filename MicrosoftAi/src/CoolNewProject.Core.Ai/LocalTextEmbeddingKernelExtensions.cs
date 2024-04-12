using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Onnx;
using Microsoft.SemanticKernel.Embeddings;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0070

namespace CoolNewProject.Core.Ai;

public static class LocalTextEmbeddingKernelExtensions {
    public static ITextEmbeddingGenerationService CreateLocalEmbeddingGenerationService(string? modelName = null,
        bool caseSensitive = false, int maximumTokens = 512) {
        modelName ??= "default";
        return BertOnnxTextEmbeddingGenerationService.Create(
            onnxModelPath: GetFullPathToModelFile(modelName, "model.onnx"),
            vocabPath:  GetFullPathToModelFile(modelName, "vocab.txt"),
            new BertOnnxOptions {
                CaseSensitive = caseSensitive,
                MaximumTokens = maximumTokens
            }
        );
    }

    public static IKernelBuilder AddLocalTextEmbeddingGeneration(
        this IKernelBuilder builder,
        string? modelName = null,
        bool caseSensitive = false,
        int maximumTokens = 512,
        string? serviceId = null) {
        builder.Services.AddKeyedSingleton(
            serviceId,
            CreateLocalEmbeddingGenerationService(modelName, caseSensitive, maximumTokens)
        );
        return builder;
    }

    private static string GetFullPathToModelFile(string modelName, string fileName) {
        string path = Path.Combine(AppContext.BaseDirectory, "LocalEmbeddingsModel", modelName, fileName);
        return File.Exists(path) ? path : throw new InvalidOperationException("Required file " + path + " does not exist");
    }
}
