using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.Threading.RateLimiting;

namespace AI.Workshop.ConsoleChat.OpenAI;

internal class EmbeddingExamples
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;

    public EmbeddingExamples()
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

    internal async Task GenerateEmbeddingsForSingleInput()
    {
        var input = "Azure OpenAI makes embedding generation easy.";

        var embedding = await _generator.GenerateAsync(input);

        Console.WriteLine($"Embedding dimension: {embedding.Vector.Length}");
        Console.WriteLine($"First 10 values: {string.Join(", ", embedding.Vector.ToArray().Take(10))}");
    }

    internal async Task GenerateEmbeddingsForMultipleInputs()
    {
        foreach (Embedding<float> embedding in await _generator.GenerateAsync(["What is AI?", "What is .NET?"]))
        {
            Console.WriteLine(string.Join(", ", embedding.Vector.ToArray().Take(5)));
        }
    }

    internal async Task GenerateVectorFromInput()
    {
        ReadOnlyMemory<float> vector = await _generator.GenerateVectorAsync("What is AI?");

        vector.ToArray().AsSpan(0, 5).ToArray().ToList()
            .ForEach(value => Console.WriteLine($"Vector value: {value}"));
    }

    internal async Task UseDelegatingEmbeddingGenerator()
    {
        var generator = new RateLimitingEmbeddingGenerator(_generator,
            new ConcurrencyLimiter(new()
            {
                PermitLimit = 1, // Limit to one concurrent request
                QueueLimit = int.MaxValue // No limit on the queue size
            }));

        var embedding = await _generator.GenerateAsync("What is .NET?");

        Console.WriteLine($"Embedding dimension: {embedding.Vector.Length}");
        Console.WriteLine($"First 5 values: {string.Join(", ", embedding.Vector.ToArray().Take(5))}");
    }
}