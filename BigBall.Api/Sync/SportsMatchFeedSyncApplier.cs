using BigBall.Domain.Entities;
using BigBall.Domain.Enums;
using BigBall.Domain.SportsData;

namespace BigBall.Api.Sync;

/// <summary>Maps authoritative <see cref="SportsMatchSnapshot"/> into persisted <see cref="Match"/>.</summary>
internal static class SportsMatchFeedSyncApplier
{
    internal static void Apply(Match entity, SportsMatchSnapshot snap, DateTime syncedUtcUtc,
        DateTime utcNowUtc)
    {
        entity.LastProviderStatusCode = snap.ProviderStatusCode;
        entity.ProviderLastSyncedUtc = syncedUtcUtc;
        entity.WentToPenalties = snap.WentToPenaltyShootout;

        if (snap.RegularTimeScoresReliable
            && snap.GoalsHomeRegularTime is not null
            && snap.GoalsAwayRegularTime is not null)
        {
            entity.ReferenceHome = snap.GoalsHomeRegularTime;
            entity.ReferenceAway = snap.GoalsAwayRegularTime;
        }

        if (snap.WentToPenaltyShootout && snap.PenaltyWinnerIsHome is { } wi)
            entity.PenaltyWinnerCode = wi ? entity.HomeCode : entity.AwayCode;
        else if (!snap.WentToPenaltyShootout)
            entity.PenaltyWinnerCode = null;

        entity.Status = DeduceDashboardStatus(entity, snap, utcNowUtc);

        // Ranking recalculation: idempotent job when authoritative result exists (wired elsewhere).
        _ = snap.ResultOrigin;
    }

    private static MatchStatus DeduceDashboardStatus(Match entity, SportsMatchSnapshot snap,
        DateTime utcNowUtc)
    {
        switch (snap.Phase)
        {
            case MatchLifecyclePhase.FinishedRegulation:
            case MatchLifecyclePhase.FinishedAfterExtraTime:
            case MatchLifecyclePhase.FinishedAfterPenalties:
            case MatchLifecyclePhase.Postponed:
            case MatchLifecyclePhase.Canceled:
            case MatchLifecyclePhase.Abandoned:
                return MatchStatus.Finished;
            case MatchLifecyclePhase.NotStarted:
                return MatchStatus.Scheduled;
            default:
                return entity.KickoffUtc <= utcNowUtc ? MatchStatus.Live : MatchStatus.Scheduled;
        }
    }
}
