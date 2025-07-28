using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;

namespace AI.Workshop.ConsoleChat.RAG.Tools;

internal class AzureAISearchInhaltIndexTool(AzureOpenAIClient client, IConfigurationRoot config, string query = null) 
    : AzureSearchToolBase(client, config, query), ISearchChatTool
{
    //public async Task<string> InvokeAsync(IDictionary<string, object> parameters = null, CancellationToken ct = default)
    //{
    //    //if (!parameters.TryGetValue("query", out var qObj) || qObj is not string query)
    //    //    throw new ArgumentException($"Missing or invalid query parameter.");

    //    return await InvokeAsync(query, 5, "inhalt-index", ct);
    //}

    public async Task<string> SearchDocumentsWithQueryAndTop(string query, int top = 5, CancellationToken ct = default)
    {
        //if (!parameters.TryGetValue("query", out var qObj) || qObj is not string query)
        //    throw new ArgumentException($"Missing or invalid query parameter.");

        return await base.InvokeAsync(query, top, "inhalt-index", ct);
    }
}
