using BigBall.Api.Configuration;
using BigBall.Domain.SportsData;

namespace BigBall.Api.Sync;

/// <summary>
/// Derives provider-agnostic spacing between outbound match-feed polls from the last canonical
/// <see cref="MatchLifecyclePhase"/>. Knobs come from <see cref="MatchProviderSyncOptions"/>.
/// </summary>
public static class MatchPollingIntervals
{
    public static TimeSpan GapForPhase(MatchProviderSyncOptions o, MatchLifecyclePhase phase)
    {
        var s = phase switch
        {
            MatchLifecyclePhase.NotStarted => o.SecondsPreMatchStale,
            MatchLifecyclePhase.FirstHalf => o.SecondsFirstHalf,
            MatchLifecyclePhase.SecondHalf => o.SecondsSecondHalf,
            MatchLifecyclePhase.Halftime => o.SecondsHalftimeBreak,
            MatchLifecyclePhase.ExtraTimeFirstHalf
                or MatchLifecyclePhase.ExtraTimeSecondHalf
                or MatchLifecyclePhase.PenaltyShootoutInProgress => o.SecondsExtraOrPenalties,
            MatchLifecyclePhase.FinishedRegulation
                or MatchLifecyclePhase.FinishedAfterExtraTime
                or MatchLifecyclePhase.FinishedAfterPenalties
                or MatchLifecyclePhase.Postponed
                or MatchLifecyclePhase.Canceled
                or MatchLifecyclePhase.Abandoned => o.SecondsTerminalReconciliation,
            MatchLifecyclePhase.Interrupted => Math.Max(30, o.SecondsHalftimeBreak),
            _ /* Unknown */ => Math.Max(30, o.SecondsHalftimeBreak),
        };

        return TimeSpan.FromSeconds(Math.Max(15, s));
    }

    /// <summary>Canonical phases where the fixture is done for poll-cadence purposes.</summary>
    public static bool LooksTerminalEnded(MatchLifecyclePhase phase)
        => phase is MatchLifecyclePhase.FinishedRegulation
                 or MatchLifecyclePhase.FinishedAfterExtraTime
                 or MatchLifecyclePhase.FinishedAfterPenalties
                 or MatchLifecyclePhase.Postponed
                 or MatchLifecyclePhase.Canceled
                 or MatchLifecyclePhase.Abandoned;

    public static bool IsDue(
        DateTime utcNowUtc,
        DateTime kickoffUtc,
        MatchLifecyclePhase lastPhase,
        DateTime? lastSyncedUtcUtc,
        MatchProviderSyncOptions o)
    {
        var warmOpensUtc = kickoffUtc - TimeSpan.FromMinutes(o.WarmWindowBeforeKickoffMinutes);
        if (utcNowUtc < warmOpensUtc)
            return false;

        var staleHorizonUtc =
            kickoffUtc + TimeSpan.FromHours(Math.Max(1, o.PollHorizonHoursAfterKickoff));
        if (LooksTerminalEnded(lastPhase) && utcNowUtc > staleHorizonUtc)
            return false;

        var wait = GapForPhase(o, lastPhase);
        return !lastSyncedUtcUtc.HasValue ||
               utcNowUtc - lastSyncedUtcUtc.Value >= wait;
    }
}
