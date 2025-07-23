using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace AI.Workshop.ConsoleChat.RAG;

internal class InMemoryVectorStoreSearch
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
    private InMemoryCollection<int, VectorModel> _cloudServicesStore;

    public InMemoryVectorStoreSearch()
    {
        var config = new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .Build();

        var endpoint = config["AZURE_OPENAI_ENDPOINT"];
        var key = config["AZURE_OPENAI_KEY"];
        var deployment = config["AZURE_EMBEDDING_DEPLOYMENT"];

        var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));

        var embeddingClient = client.GetEmbeddingClient(deployment);

        _generator = embeddingClient.AsIEmbeddingGenerator();
    }

    internal async Task GenerateVectorsAsync()
    {
        // Create and populate a vector store with the cloud service data.
        // Use the IEmbeddingGenerator implementation to create and assign an embedding vector
        // for each record in the cloud service data:

        var vectorStore = new InMemoryVectorStore();
        _cloudServicesStore = vectorStore.GetCollection<int, VectorModel>("cloudServices");
        await _cloudServicesStore.EnsureCollectionExistsAsync();

        foreach (var service in Data.CloudServices)
        {
            service.Vector = await _generator.GenerateVectorAsync(service.Description);
            await _cloudServicesStore.UpsertAsync(service);
        }
    }

    public async Task SearchAsync(string text, int numberOfResults = 1)
    {
        // Create an embedding for a search query and use it to perform a vector search on the vector store:

        var queryEmbedding = await _generator.GenerateVectorAsync(text);

        var results = _cloudServicesStore.SearchAsync(queryEmbedding, top: numberOfResults);

        // The app prints out the top result of the vector search,
        // which is the cloud service that's most relevant to the original query:

        await foreach (var result in results)
        {
            Console.WriteLine($"Name: {result.Record.Name}");
            Console.WriteLine($"Description: {result.Record.Description}");
            Console.WriteLine($"Vector match score: {result.Score}");
        }
    }
}
