namespace AI.Workshop.ChatWebApp.Tools;

internal class CurrentTimeTool : IChatTool
{
    public async Task<string> SearchDocumentsWithQueryAndTop(string query, int top = 5, CancellationToken ct = default)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
        return now.ToString("dd.MM.yyyyTHH:mm:ssK");
    }
}
