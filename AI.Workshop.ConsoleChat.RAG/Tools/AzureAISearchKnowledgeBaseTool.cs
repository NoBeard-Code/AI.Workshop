using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;

namespace AI.Workshop.ConsoleChat.RAG.Tools;

internal class AzureAISearchKnowledgeBaseTool(AzureOpenAIClient client, IConfigurationRoot config, string query = null)
    : AzureSearchToolBase(client, config, query), IChatTool, ISearchChatTool
{
    public async Task<string> InvokeAsync(IDictionary<string, object> parameters = null, CancellationToken ct = default)
    {
        //if (!parameters.TryGetValue("query", out var qObj) || qObj is not string query)
        //    throw new ArgumentException($"Missing or invalid query parameter.");

        return await InvokeAsync(query, 5, "knowledge-base", ct);
    }

    [Description("This tool searches through Azure AISearch Knowledge base index to find relevant context information to help answer user query.")]
    public async Task<string> SearchDocumentsWithQueryAndTop(
        [Description("The query to search for in the Azure Search Knowledge base index.")]
        string query,
        [Description("The number of documents you wish to retrieve from the search index.")]
        int top = 5,
        CancellationToken ct = default)
    {
        //if (!parameters.TryGetValue("query", out var qObj) || qObj is not string query)
        //    throw new ArgumentException($"Missing or invalid query parameter.");

        return await base.InvokeAsync(query, 5, "knowledge-base", ct);
    }
}
