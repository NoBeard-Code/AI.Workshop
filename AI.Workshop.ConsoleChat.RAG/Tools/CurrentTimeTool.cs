using System.ComponentModel;

namespace AI.Workshop.ConsoleChat.RAG.Tools;

internal static class CurrentTimeTool
{
    [Description("Returns the current date and time for Central European Time Zone. This tool needs no parameters.")]
    internal static string GetTime()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
        return now.ToString("dd.MM.yyyyTHH:mm:ssK");
    }
}
