﻿using System.ComponentModel;

namespace AI.Workshop.ConsoleChat.RAG.Tools;

internal class CurrentTimeTool : IChatTool
{
    [Description("Returns the current date and time for Central European Time Zone. This tool needs no parameters.")]
    public async Task<string> InvokeAsync(IDictionary<string, object> parameters = null, CancellationToken ct = default)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
        return now.ToString("dd.MM.yyyyTHH:mm:ssK");
    }
}
