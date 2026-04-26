namespace BigBall.Web.Shared.UI;

public static class TimeFormatting
{
    public static string FormatCountdown(int seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);
        return t.TotalHours >= 1
            ? $"{(int)t.TotalHours:D2}:{t.Minutes:D2}:{t.Seconds:D2}"
            : $"{t.Minutes:D2}:{t.Seconds:D2}";
    }

    /// <summary>
    /// Formats a UTC datetime as a relative countdown (e.g., "em 2h 30m", "em 3d")
    /// </summary>
    public static string FormatKickoff(DateTime utc)
    {
        var delta = utc - DateTime.UtcNow;
        if (delta <= TimeSpan.Zero) return "agora";
        if (delta.TotalHours < 24) return $"em {(int)delta.TotalHours}h {delta.Minutes:D2}m";
        return $"em {(int)delta.TotalDays}d";
    }
}
