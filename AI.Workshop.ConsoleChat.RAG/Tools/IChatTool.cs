namespace AI.Workshop.ConsoleChat.RAG.Tools;

internal interface IChatTool
{
    Task<string> InvokeAsync(IDictionary<string, object> parameters = null, CancellationToken ct = default);
}
