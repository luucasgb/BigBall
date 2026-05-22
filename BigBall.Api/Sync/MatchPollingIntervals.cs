using BigBall.Api.Configuration;

namespace BigBall.Api.Sync;

/// <summary>
/// Derives spacing between outbound SportsAPI polls from last vendor status (<see cref="SportsApiProSyncOptions"/>).
/// </summary>
public static class MatchPollingIntervals
{
    public static TimeSpan GapForVendorStatus(SportsApiProSyncOptions o, int? statusCode)
    {
        var s = statusCode switch
        {
            null or 80 => Math.Max(30, o.SecondsHalftimeBreak),
            0 => o.SecondsPreMatchStale,
            6 => o.SecondsFirstHalf,
            7 => o.SecondsSecondHalf,
            31 => o.SecondsHalftimeBreak,
            40 or 41 or 50 => o.SecondsExtraOrPenalties,
            110 or 100 or 120 or 60 or 70 or 90 => o.SecondsTerminalReconciliation,
            _ => Math.Max(o.SecondsFirstHalf, o.SecondsSecondHalf)
        };

        return TimeSpan.FromSeconds(Math.Max(15, s));
    }

    /// <summary>Vendor codes where the fixture is done for poll purposes (SportsApiProMapper).</summary>
    public static bool LooksTerminalEnded(int? code)
        => code is 100 or 110 or 120 or 60 or 70 or 90;

    public static bool IsDue(DateTime utcNowUtc, DateTime kickoffUtc, int? lastCode, DateTime? lastSyncedUtcUtc,
        SportsApiProSyncOptions o)
    {
        var warmOpensUtc = kickoffUtc - TimeSpan.FromMinutes(o.WarmWindowBeforeKickoffMinutes);
        if (utcNowUtc < warmOpensUtc)
            return false;

        var staleHorizonUtc =
            kickoffUtc + TimeSpan.FromHours(Math.Max(1, o.PollHorizonHoursAfterKickoff));
        if (LooksTerminalEnded(lastCode) && utcNowUtc > staleHorizonUtc)
            return false;

        var wait = GapForVendorStatus(o, lastCode);
        return !lastSyncedUtcUtc.HasValue ||
               utcNowUtc - lastSyncedUtcUtc.Value >= wait;
    }
}
