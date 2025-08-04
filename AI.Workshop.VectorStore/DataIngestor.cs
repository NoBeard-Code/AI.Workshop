using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace AI.Workshop.VectorStore;

public class DataIngestor(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    private InMemoryCollection<int, VectorModel> _inMemoryVectorStore;

    public async Task IngestDataAsync()
    {
        var vectorStore = new InMemoryVectorStore();
        _inMemoryVectorStore = vectorStore.GetCollection<int, VectorModel>("cloudServices");
        await _inMemoryVectorStore.EnsureCollectionExistsAsync();

        foreach (var service in SampleData.CloudServices)
        {
            service.Vector = await embeddingGenerator.GenerateVectorAsync(service.Description);
            await _inMemoryVectorStore.UpsertAsync(service);
        }
    }

    public async IAsyncEnumerable<VectorSearchResult<VectorModel>> SearchAsync(string text, int numberOfResults = 1)
    {
        var queryEmbedding = await embeddingGenerator.GenerateVectorAsync(text);

        var results = _inMemoryVectorStore.SearchAsync(queryEmbedding, top: numberOfResults);

        await foreach (var result in results)
        {
            Console.WriteLine($"Name: {result.Record.Name}");
            Console.WriteLine($"Description: {result.Record.Description}");
            Console.WriteLine($"Vector match score: {result.Score}");
            yield return result;
        }
    }
}
