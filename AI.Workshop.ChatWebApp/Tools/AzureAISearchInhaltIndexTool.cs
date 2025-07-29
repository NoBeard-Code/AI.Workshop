using Microsoft.Extensions.AI;

namespace AI.Workshop.ChatWebApp.Tools;

internal class AzureAISearchInhaltIndexTool(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, 
    IConfiguration config, string query = null) : AzureSearchToolBase(embeddingGenerator, config, query), IChatTool
{
    public async Task<string> SearchDocumentsWithQueryAndTop(string query, int top = 5, CancellationToken ct = default)
    {
        return await base.InvokeAsync(query, top, "inhalt-index", ct);
    }
}
