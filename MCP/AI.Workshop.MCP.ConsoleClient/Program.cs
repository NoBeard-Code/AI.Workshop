using Microsoft.Extensions.AI;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using OllamaSharp;
using System.Text;

//var transport = new StdioClientTransport(new StdioClientTransportOptions()
//{
//    Name = "MCP Console Client",
//    Command = @"C:\Projects\AI\AI.Workshop\MCP\AI.Workshop.MCP.ConsoleServer\bin\Debug\net9.0\AI.Workshop.MCP.ConsoleServer.exe"
//});

var serverDir = Path.GetFullPath(AppContext.BaseDirectory.Replace("ConsoleClient", "ConsoleServer"));
var serverDll = Path.Combine(serverDir, "AI.Workshop.MCP.ConsoleServer.dll");

var transport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "MCP Console Client",
    Command = "dotnet", // Launch via dotnet CLI
    Arguments = [serverDll],
    WorkingDirectory = serverDir,
    EnvironmentVariables = new Dictionary<string, string?>
    {
        { "DOTNET_ENVIRONMENT", "Development" },
        { "MCP_LOG_LEVEL", "Debug" }
    }
});

var client = await McpClientFactory.CreateAsync(transport);

IList<McpClientTool> tools = await client.ListToolsAsync();

foreach (var tool in tools)
{
    Console.WriteLine($"Found tool: {tool.Name} - {tool.Description}");
}

var result = await client.CallToolAsync("echo", new Dictionary<string, object?>() { ["message"] = "Hello MCP!" });

Console.WriteLine($"Result: {result.Content.First().ToAIContent()}");

result = await client.CallToolAsync("reverse_echo", new Dictionary<string, object?>() { ["message"] = "Hello MCP!" });

Console.WriteLine($"Result: {result.Content.First().ToAIContent()}");

var ollamaUri = new Uri("http://localhost:11434/");
var ollamaModel = "llama3.2";

var chatClient = new OllamaApiClient(ollamaUri, ollamaModel);

var clientBuilder = new ChatClientBuilder(chatClient)
    .UseFunctionInvocation()
    .Build();

var systemPrompt = @"
            You are a helpful assistant that suggests which monkeys are available in the database.
        ";

List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];

var chatOptions = new ChatOptions
{
    Tools = [.. tools]
};

var userPrompt = "get me the locations of the monkeys in the db";

var response = await clientBuilder.GetResponseAsync(userPrompt, chatOptions);

Console.WriteLine($"Response: {response}");

Console.ReadKey();