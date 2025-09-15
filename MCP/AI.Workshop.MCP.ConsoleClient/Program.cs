using ModelContextProtocol;
using ModelContextProtocol.Client;

var transport = new StdioClientTransport(new StdioClientTransportOptions()
{
    Name = "MCP Console Client",
    Command = @"C:\Projects\AI\AI.Workshop\MCP\AI.Workshop.MCP.ConsoleServer\bin\Debug\net9.0\AI.Workshop.MCP.ConsoleServer.exe"
});

var client = await McpClientFactory.CreateAsync(transport);

foreach (var tool in await client.ListToolsAsync())
{
    Console.WriteLine($"Found tool: {tool.Name} - {tool.Description}");
}

var result = await client.CallToolAsync("echo", new Dictionary<string, object?>() { ["message"] = "Hello MCP!" });

Console.WriteLine($"Result: {result.Content.First().ToAIContent()}");

result = await client.CallToolAsync("reverse_echo", new Dictionary<string, object?>() { ["message"] = "Hello MCP!" });

Console.WriteLine($"Result: {result.Content.First().ToAIContent()}");

Console.ReadKey();