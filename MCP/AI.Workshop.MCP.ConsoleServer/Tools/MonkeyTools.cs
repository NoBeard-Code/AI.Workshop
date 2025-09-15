using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace AI.Workshop.MCP.ConsoleServer.Tools;

[McpServerToolType]
public static class MonkeyTools
{
    [McpServerTool, Description("Get a list of monkeys.")]
    public static async Task<string> GetMonkeys(MonkeyService monkeyService)
    {
        var monkeys = await monkeyService.GetMonkeysAsync();
        return JsonSerializer.Serialize(monkeys);
    }

    [McpServerTool, Description("Get a monkey by name.")]
    public static async Task<string> GetMonkey(MonkeyService monkeyService, [Description("The name of the monkey to get details for")] string name)
    {
        var monkey = await monkeyService.GetMonkeyAsync(name);
        return JsonSerializer.Serialize(monkey);
    }
}
