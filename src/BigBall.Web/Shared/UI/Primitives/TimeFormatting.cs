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
    /// Human-readable time until kickoff. Tiers: &gt;7d → days only; &gt;12h → days+hours;
    /// &gt;5m → hours+minutes; else → minutes+seconds. Exactly 7d uses the 12h tier (7d 0h).
    /// </summary>
    public static string FormatPredictionDeadlineCountdown(TimeSpan remaining)
    {
        if (remaining <= TimeSpan.Zero)
        {
            return "0m";
        }

        if (remaining > TimeSpan.FromDays(7))
        {
            return FormatDaysOnly(remaining);
        }

        if (remaining > TimeSpan.FromHours(12))
        {
            return FormatDaysAndHours(remaining);
        }

        if (remaining > TimeSpan.FromMinutes(5))
        {
            return FormatHoursAndMinutes(remaining);
        }

        return FormatMinutesAndSeconds(remaining);
    }

    private static string FormatDaysOnly(TimeSpan remaining) =>
        $"{(int)Math.Floor(remaining.TotalDays)}d";

    private static string FormatDaysAndHours(TimeSpan remaining) =>
        $"{remaining.Days}d {remaining.Hours}h";

    private static string FormatHoursAndMinutes(TimeSpan remaining) =>
        $"{(int)remaining.TotalHours}h {remaining.Minutes}m";

    private static string FormatMinutesAndSeconds(TimeSpan remaining)
    {
        var t = (int)remaining.TotalSeconds;
        return $"{t / 60}m {t % 60:D2}s";
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
