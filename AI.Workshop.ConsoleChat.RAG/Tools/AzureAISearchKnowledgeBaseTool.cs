using System.Text;

namespace AI.Workshop.ConsoleChat.RAG.Tools;

internal class AzureAISearchKnowledgeBaseTool : IChatTool
{
    public async Task<string> InvokeAsync(IDictionary<string, object> parameters = null, CancellationToken ct = default)
    {
        // Simulate a search operation
        // In a real implementation, you would call the Azure AI Search service here

        await Task.Delay(100); // Simulating async operation

        var sb = new StringBuilder();
        sb.AppendLine("Context from documents in the database:\n");

        // Mock results for demonstration purposes
        sb.AppendLine("<MOCKED DATA>");

        return sb.ToString();
    }
}
