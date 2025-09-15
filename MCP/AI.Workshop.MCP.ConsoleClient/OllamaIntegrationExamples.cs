using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OllamaSharp;

namespace AI.Workshop.MCP.ConsoleClient;

internal class OllamaIntegrationExamples
{
    private readonly IChatClient _chatClient;

    public OllamaIntegrationExamples()
    {
        var ollamaUri = new Uri("http://localhost:11434/");
        var ollamaModel = "llama3.2";

        _chatClient = new OllamaApiClient(ollamaUri, ollamaModel);
    }

    internal async Task BasicRagWithMcpToolsAsync()
    {
        var clientBuilder = new ChatClientBuilder(_chatClient)
            .UseFunctionInvocation()
            .Build();

        var systemPrompt = @"
            You are a helpful assistant that suggests which monkeys are available in the database.
        ";

        List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];

        var sampleClient = new SampleMcpClient();
        var client = await sampleClient.GetClientAsync();

        var tools = await client.ListToolsAsync();

        var chatOptions = new ChatOptions
        {
            Tools = [.. tools]
        };

        var userPrompt = "get me the locations of the monkeys in the db";

        var response = await clientBuilder.GetResponseAsync(userPrompt, chatOptions);

        Console.WriteLine($"Response: {response}");
    }
}
