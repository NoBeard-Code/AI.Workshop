using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;

namespace AI.Workshop.MCP.ConsoleClient;

/// <summary>
/// https://www.youtube.com/watch?v=9cwSOyavdSI&t=3223s
/// </summary>
internal class GitHubMcpService
{
    private readonly StdioClientTransport _transport;
    private static IMcpClient? _mcpClient;
    private readonly string[] _toolsToUse = 
        ["search_repositories", "get_file_contents", "list_issues", "search_code", "search_issues", "search_users"];

    public GitHubMcpService()
    {
        if (_mcpClient != null)
            return;

        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var pat = config["GITHUB_PERSONAL_ACCESS_TOKEN"];

        _transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "GitHub",
            Command = "npx",
            Arguments = ["-y", "@modelcontextprotocol/server-github"],
            EnvironmentVariables = new Dictionary<string, string?>
            {
                { "GITHUB_PERSONAL_ACCESS_TOKEN", pat }
            },
            ShutdownTimeout = TimeSpan.FromSeconds(30)
        });
    }

    public async Task<IMcpClient> GetClientAsync()
    {
        var options = new McpClientOptions()
        {
            ClientInfo = new() { Name = "TestClient", Version = "1.0.0" },
            InitializationTimeout = TimeSpan.FromSeconds(30)
        };

        _mcpClient ??= await McpClientFactory.CreateAsync(_transport, options);

        return _mcpClient;
    }

    public async Task<IEnumerable<AIFunction>> GetToolsAsync()
    {
        var client = await GetClientAsync();
        var allTools = await client.ListToolsAsync();
        return allTools.Where(tool => _toolsToUse.Contains(tool.Name));
    }
}
