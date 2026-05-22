using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BigBall.Shared.WorldCup;

/// <summary>Parses openfootball <c>worldcup.json</c> schedule fields and maps round labels to phases.</summary>
public static class OpenFootballScheduleParse
{
    private static readonly Regex TimeWithUtcOffset = new(
        @"^(\d{1,2}:\d{2})(?::(\d{2}))?\s+UTC([+-]\d+)$",
        RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Parses <paramref name="date"/> (yyyy-MM-dd) and <paramref name="time"/> (e.g. <c>13:00 UTC-6</c>, <c>19:30 UTC-4</c>).
    /// </summary>
    public static bool TryParseKickoffUtc(string date, string time, out DateTime kickoffUtc)
    {
        kickoffUtc = default;
        if (string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(time))
        {
            return false;
        }

        if (!DateOnly.TryParseExact(date.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
        {
            return false;
        }

        var m = TimeWithUtcOffset.Match(time.Trim());
        if (!m.Success)
        {
            return false;
        }

        var hm = m.Groups[1].Value.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (hm.Length < 2)
        {
            return false;
        }

        var h = int.Parse(hm[0], CultureInfo.InvariantCulture);
        var min = int.Parse(hm[1], CultureInfo.InvariantCulture);
        int sec = m.Groups[2].Success ? int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture) : 0;
        int offsetHours = int.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);

        // Local = wall + offset to UTC. Example: 13:00 at UTC-6 => 19:00 UTC
        var offset = TimeSpan.FromHours(offsetHours);
        var local = new DateTime(d.Year, d.Month, d.Day, h, min, sec, DateTimeKind.Unspecified);
        var dto = new DateTimeOffset(local, offset);
        kickoffUtc = dto.UtcDateTime;
        return true;
    }

    /// <summary>Maps <c>round</c> from worldcup.json to a string that matches <c>MatchPhase</c> enum names.</summary>
    public static string MapRoundToPhaseString(string? round)
    {
        if (string.IsNullOrWhiteSpace(round))
        {
            return "Groups";
        }

        var r = round.Trim();
        if (r.Contains("Matchday", StringComparison.OrdinalIgnoreCase))
        {
            return "Groups";
        }

        if (r.Equals("Round of 32", StringComparison.OrdinalIgnoreCase))
        {
            return "RoundOf32";
        }

        if (r.Equals("Round of 16", StringComparison.OrdinalIgnoreCase))
        {
            return "RoundOf16";
        }

        if (r.Equals("Quarter-final", StringComparison.OrdinalIgnoreCase))
        {
            return "Quarters";
        }

        if (r.Equals("Semi-final", StringComparison.OrdinalIgnoreCase))
        {
            return "Semis";
        }

        if (r.Equals("Match for third place", StringComparison.OrdinalIgnoreCase))
        {
            return "ThirdPlace";
        }

        if (r.Equals("Final", StringComparison.OrdinalIgnoreCase))
        {
            return "Final";
        }

        return "Groups";
    }

    /// <summary>Stable <c>wc2026-…</c> key: numeric knockout slot or hash for group/odd rows.</summary>
    public static string BuildExternalKey(int? num, string date, string time, string team1, string team2, string round)
    {
        if (num is int n)
        {
            return "wc2026-" + n;
        }

        // Third place and Final have no num in the source; distinguish by date+round+teams
        var payload = string.Join("|", date, time, team1, team2, round);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        var b64 = Convert.ToHexString(hash.AsSpan(0, 8)).ToLowerInvariant();
        return "wc2026-h" + b64;
    }
}
