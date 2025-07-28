namespace AI.Workshop.ConsoleChat.RAG.Tools;

internal interface ISearchChatTool
{
    Task<string> InvokeFixedAsync(string query, int top = 5, CancellationToken ct = default);
}
