using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace AI.Workshop.ConsoleChat.OpenAI;

internal class ChatClientBuilderExamples
{
    private readonly IChatClient _client;

    internal ChatClientBuilderExamples()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var endpoint = config["AZURE_OPENAI_ENDPOINT"];
        var key = config["AZURE_OPENAI_KEY"];
        var deployment = config["AZURE_OPENAI_DEPLOYMENT"];

        _client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key))
            .GetChatClient(deployment)
            .AsIChatClient();
    }

    internal async Task ToolCalling()
    {
        string GetCurrentWeather() => Random.Shared.NextDouble() > 0.5 ? "It's sunny" : "It's raining";

        var clientBuilder = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        ChatOptions options = new() { Tools = [AIFunctionFactory.Create(GetCurrentWeather)] };

        await foreach (var update in clientBuilder.GetStreamingResponseAsync("Should I wear a rain coat?", options))
        {
            Console.Write(update);
        }

        Console.WriteLine();
    }

    internal async Task DistributedCache()
    {
        var cache = new MemoryDistributedCache(
            Options.Create(new MemoryDistributedCacheOptions())
            );

        var clientBuilder = new ChatClientBuilder(_client)
            .UseDistributedCache(cache)
            .Build();

        string[] prompts = ["What is AI?", "What is .NET?", "What is AI?"];

        foreach (var prompt in prompts)
        {
            await foreach (var update in _client.GetStreamingResponseAsync(prompt))
            {
                Console.Write(update);
            }
            Console.WriteLine();
        }
    }
}
