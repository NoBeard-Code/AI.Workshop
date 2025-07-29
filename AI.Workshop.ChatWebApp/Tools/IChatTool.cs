namespace AI.Workshop.ChatWebApp.Tools;

internal interface IChatTool
{
    Task<string> SearchDocumentsWithQueryAndTop(string query, int top = 5, CancellationToken ct = default);
}
