﻿using Microsoft.Extensions.AI;
using OllamaSharp;
using System.Text;

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

        var workshopMcp = new WorkshopMcpService();
        var mcpTools = await workshopMcp.GetToolsAsync();

        var chatOptions = new ChatOptions
        {
            Tools = [.. mcpTools]
        };

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(systemPrompt);
        Console.ResetColor();

        while (true)
        {
            // Get input
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nQ: ");
            var input = Console.ReadLine()!;

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Exiting chat.");
                Console.ResetColor();
                break;
            }

            history.Add(new(ChatRole.User, input));

            var streamingResponse = clientBuilder.GetStreamingResponseAsync(history, chatOptions);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("A: ");
            var messageBuilder = new StringBuilder();

            await foreach (var chunk in streamingResponse)
            {
                Console.Write(chunk.Text);
                messageBuilder.Append(chunk.Text);
            }

            history.Add(new(ChatRole.Assistant, messageBuilder.ToString()));
            Console.ResetColor();
        }
    }

    internal async Task RagWithToolsFromGitHubServerAsync()
    {
        var clientBuilder = new ChatClientBuilder(_chatClient)
            .UseFunctionInvocation()
            .Build();

        var systemPrompt = @"
            You are a helpful assistant that provides information about my code on the GitHub.
        ";

        List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];

        var gitHubMcp = new GitHubMcpService();
        var gitHubTools = await gitHubMcp.GetToolsAsync();

        var chatOptions = new ChatOptions
        {
            Tools = [.. gitHubTools ]
        };

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(systemPrompt);
        Console.ResetColor();

        while (true)
        {
            // Get input
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nQ: ");
            var input = Console.ReadLine()!;

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Exiting chat.");
                Console.ResetColor();
                break;
            }

            history.Add(new(ChatRole.User, input));

            var streamingResponse = clientBuilder.GetStreamingResponseAsync(history, chatOptions);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("A: ");
            var messageBuilder = new StringBuilder();

            await foreach (var chunk in streamingResponse)
            {
                Console.Write(chunk.Text);
                messageBuilder.Append(chunk.Text);
            }

            history.Add(new(ChatRole.Assistant, messageBuilder.ToString()));
            Console.ResetColor();
        }
    }
}
