using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AI.Workshop.MCP.ConsoleServer;

/// <summary>
/// https://devblogs.microsoft.com/dotnet/build-a-model-context-protocol-mcp-server-in-csharp/
/// </summary>
public class MonkeyService(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient();

    List<Monkey> _monkeyList = [];

    public async Task<List<Monkey>> GetMonkeysAsync()
    {
        if (_monkeyList?.Count > 0)
            return _monkeyList;

        var response = await httpClient.GetAsync("https://www.montemagno.com/monkeys.json");
        if (response.IsSuccessStatusCode)
        {
            _monkeyList = await response.Content.ReadFromJsonAsync(MonkeyContext.Default.ListMonkey) ?? [];
        }

        _monkeyList ??= [];

        return _monkeyList;
    }

    public async Task<Monkey?> GetMonkeyAsync(string name)
    {
        var monkeys = await GetMonkeysAsync();
        return monkeys.FirstOrDefault(m => m.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
    }
}

public partial class Monkey
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? Details { get; set; }
    public string? Image { get; set; }
    public int Population { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

[JsonSerializable(typeof(List<Monkey>))]
internal sealed partial class MonkeyContext : JsonSerializerContext
{

}
