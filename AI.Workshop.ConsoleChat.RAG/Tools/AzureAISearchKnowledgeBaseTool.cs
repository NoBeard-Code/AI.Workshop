using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;

namespace AI.Workshop.ConsoleChat.RAG.Tools;

internal class AzureAISearchKnowledgeBaseTool(AzureOpenAIClient client, IConfigurationRoot config)
    : AzureSearchToolBase(client, config), IChatTool
{
    public async Task<string> InvokeAsync(IDictionary<string, object> parameters = null, CancellationToken ct = default)
    {
        if (!parameters.TryGetValue("query", out var qObj) || qObj is not string query)
            throw new ArgumentException($"Missing or invalid query parameter.");

        return await InvokeAsync(query, "knowledge-base", ct);
    }
}
