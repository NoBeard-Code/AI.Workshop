﻿using ModelContextProtocol;
using ModelContextProtocol.Client;

namespace AI.Workshop.MCP.ConsoleClient;

/// <summary>
/// https://github.com/modelcontextprotocol/csharp-sdk
/// https://devblogs.microsoft.com/dotnet/build-a-model-context-protocol-mcp-server-in-csharp/
/// https://github.com/jamesmontemagno/MonkeyMCP
/// </summary>
internal class McpServerStdioExamples
{
    internal async Task EnlistServerInfoAsync()
    {
        var sampleClient = new WorkshopMcpService();
        var client = await sampleClient.GetClientAsync();

        await client.PingAsync();

        Console.WriteLine($"Capability: {nameof(client.ServerCapabilities.Resources)} - {client.ServerCapabilities.Resources}");
        Console.WriteLine($"Capability: {nameof(client.ServerCapabilities.Prompts)} - {client.ServerCapabilities.Prompts}");
        Console.WriteLine($"Capability: {nameof(client.ServerCapabilities.Tools)} - {client.ServerCapabilities.Tools}");
        Console.WriteLine($"Capability: {nameof(client.ServerCapabilities.Completions)} - {client.ServerCapabilities.Completions}");
        Console.WriteLine($"Capability: {nameof(client.ServerCapabilities.Logging)} - {client.ServerCapabilities.Logging}");
        Console.WriteLine($"Capability: {nameof(client.ServerCapabilities.NotificationHandlers)} - {client.ServerCapabilities.NotificationHandlers}");
        Console.WriteLine($"Capability: {nameof(client.ServerCapabilities.Experimental)} - {client.ServerCapabilities.Experimental}");

        if (client.ServerCapabilities.Prompts != null)
        {
            foreach (var prompt in await client.ListPromptsAsync())
            {
                Console.WriteLine($"Prompt: {prompt.Name} - {prompt.Description}");
            }
        }

        if (client.ServerCapabilities.Resources != null)
        {
            foreach (var resource in await client.ListResourcesAsync())
            {
                Console.WriteLine($"Resource: {resource.Name} - {resource.Description}");
            }

            foreach (var template in await client.ListResourceTemplatesAsync())
            {
                Console.WriteLine($"Template: {template.Name} - {template.Description}");
            }
        }

        IList<McpClientTool> tools = await client.ListToolsAsync();
        foreach (var tool in tools)
        {
            Console.WriteLine($"Found tool: {tool.Name} - {tool.Description}");
        }
    }

    internal async Task CallMcpServerToolsAsync()
    {
        var sampleClient = new WorkshopMcpService();
        var client = await sampleClient.GetClientAsync();

        var result = await client.CallToolAsync("echo", new Dictionary<string, object?>() { ["message"] = "Hello MCP!" });

        Console.WriteLine($"Result: {result.Content.First().ToAIContent()}");

        result = await client.CallToolAsync("reverse_echo", new Dictionary<string, object?>() { ["message"] = "Hello MCP!" });

        Console.WriteLine($"Result: {result.Content.First().ToAIContent()}");
    }

    internal async Task CallMonkeyToolsAsync()
    {
        var sampleClient = new WorkshopMcpService();
        var client = await sampleClient.GetClientAsync();

        var result = await client.CallToolAsync("get_monkeys", new Dictionary<string, object?>());
        Console.WriteLine($"Result: {result.Content.First().ToAIContent()}");

        result = await client.CallToolAsync("get_monkey", new Dictionary<string, object?>() { ["name"] = "Baboon" });
        Console.WriteLine($"Result: {result.Content.First().ToAIContent()}");
    }
}
