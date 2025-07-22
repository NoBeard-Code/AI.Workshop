using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace AI.Workshop.ConsoleChat.OpenAI;

// https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai

internal class ChatClientBuilderExamples : AzureOpenAIBase
{
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

    internal async Task UseTelemetry()
    {
        var sourceName = "AI.Workshop.ConsoleChat.OpenAI";

        var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource(sourceName)
            .AddConsoleExporter()
            .Build();

        var clientBuilder = new ChatClientBuilder(_client)
            .UseOpenTelemetry(
                sourceName: sourceName,
                configure: c => c.EnableSensitiveData = true)
            .Build();

        Console.WriteLine((await _client.GetResponseAsync("What is AI?")).Text);
    }
}
