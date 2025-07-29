using Microsoft.Extensions.AI;

namespace AI.Workshop.ChatWebApp.Tools;

internal class AzureAISearchKnowledgeBaseTool(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IConfiguration config, string query = null) : AzureSearchToolBase(embeddingGenerator, config, query), IChatTool
{
    public async Task<string> SearchDocumentsWithQueryAndTop(string query, int top = 5, CancellationToken ct = default)
    {
        return await InvokeAsync(query, top, "knowledge-base", ct);
    }
}
