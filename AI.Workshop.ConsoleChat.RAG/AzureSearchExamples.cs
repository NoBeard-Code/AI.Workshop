using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace AI.Workshop.ConsoleChat.RAG;

internal class AzureSearchExamples
{
    private readonly AzureOpenAIClient _client;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
    protected readonly IChatClient _chatClient;

    public AzureSearchExamples()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .AddJsonFile("Prompts.json", false, false)
            .Build();

        var openAiEndpoint = config["AZURE_OPENAI_ENDPOINT"];
        var openAiKey = config["AZURE_OPENAI_KEY"];
        var deployment = config["AZURE_EMBEDDING_DEPLOYMENT"];
        var searchEndpoint = config["AZURE_SEARCH_ENDPOINT"];
        var searchKey = config["AZURE_SEARCH_KEY"];

        _client = new AzureOpenAIClient(new Uri(openAiEndpoint), new AzureKeyCredential(openAiKey));

        var embeddingClient = _client.GetEmbeddingClient(deployment);

        _generator = embeddingClient.AsIEmbeddingGenerator();
    }

    internal async Task GenerateEmbeddingForQueryAsync(string text)
    {
        var embedding = await _generator.GenerateAsync(text);

        Console.WriteLine($"Embedding dimension: {embedding.Vector.Length}");
        Console.WriteLine($"First 10 values: {string.Join(", ", embedding.Vector.ToArray().Take(10))}");

        ReadOnlyMemory<float> vector = await _generator.GenerateVectorAsync(text);

        vector.ToArray().AsSpan(0, 5).ToArray().ToList()
            .ForEach(value => Console.WriteLine($"Vector value: {value}"));
    }
}
