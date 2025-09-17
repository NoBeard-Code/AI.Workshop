using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace AI.Workshop.MCP.ConsoleClient;

internal class WorkshopMcpService
{
    private readonly StdioClientTransport _transport;
    private static IMcpClient? _mcpClient;

    public WorkshopMcpService()
    {
        if (_mcpClient != null)
            return;

        //var transport = new StdioClientTransport(new StdioClientTransportOptions()
        //{
        //    Name = "MCP Console Client",
        //    Command = @"C:\Projects\AI\AI.Workshop\MCP\AI.Workshop.MCP.ConsoleServer\bin\Debug\net9.0\AI.Workshop.MCP.ConsoleServer.exe"
        //});

        var serverDir = Path.GetFullPath(AppContext.BaseDirectory.Replace("ConsoleClient", "ConsoleServer"));
        var serverDll = Path.Combine(serverDir, "AI.Workshop.MCP.ConsoleServer.dll");

        _transport = new StdioClientTransport(new StdioClientTransportOptions
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
    }

    public async Task<IMcpClient> GetClientAsync()
    {
        _mcpClient ??= await McpClientFactory.CreateAsync(_transport);

        return _mcpClient;
    }

    public async Task<IEnumerable<AIFunction>> GetToolsAsync()
    {
        var client = await GetClientAsync();
        return await client.ListToolsAsync();
    }
}
