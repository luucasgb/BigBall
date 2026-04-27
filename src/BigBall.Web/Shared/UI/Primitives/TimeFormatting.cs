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

    /// <summary>Human-readable time until kickoff (prediction deadline). When <paramref name="includeSeconds"/> is true (last 5 minutes), includes seconds.</summary>
    public static string FormatPredictionDeadlineCountdown(TimeSpan remaining, bool includeSeconds)
    {
        if (remaining <= TimeSpan.Zero)
        {
            return "0m";
        }

        if (includeSeconds)
        {
            var totalSec = (int)remaining.TotalSeconds;
            var m = totalSec / 60;
            var s = totalSec % 60;
            return $"{m}m {s:D2}s";
        }

        var d = (int)remaining.TotalDays;
        if (d > 0)
        {
            return $"{d}d {remaining.Hours}h {remaining.Minutes}m";
        }

        var totalH = (int)remaining.TotalHours;
        if (totalH > 0)
        {
            return $"{totalH}h {remaining.Minutes}m";
        }

        return $"{remaining.Minutes}m";
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
