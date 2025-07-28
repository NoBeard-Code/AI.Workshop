using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;

namespace AI.Workshop.ConsoleChat.RAG.Tools;

internal class AzureAISearchInhaltIndexTool(AzureOpenAIClient client, IConfigurationRoot config, string query = null) 
    : AzureSearchToolBase(client, config, query), IChatTool, ISearchChatTool
{
    public async Task<string> InvokeAsync(IDictionary<string, object> parameters = null, CancellationToken ct = default)
    {
        if (!parameters.TryGetValue("query", out var qObj) || qObj is not string query)
            throw new ArgumentException($"Missing or invalid query parameter.");

        return await InvokeAsync(query, 5, "inhalt-index", ct);
    }

    [Description("This tool searches through Azure AISearch index of available seminars to help you recommend a seminar to the user.")]
    public async Task<string> SearchDocumentsWithQueryAndTop(
        [Description("The query to search for in the Azure Search index and get seminar with semantically similar description.")]
        string query,
        [Description("The number of seminars you wish to retrieve from the search index.")]
        int top = 5, 
        CancellationToken ct = default)
    {
        //if (!parameters.TryGetValue("query", out var qObj) || qObj is not string query)
        //    throw new ArgumentException($"Missing or invalid query parameter.");

        return await base.InvokeAsync(query, top, "inhalt-index", ct);
    }
}
