using BigBall.Api.Data;
using BigBall.Domain.Entities;
using BigBall.Domain.Enums;
using BigBall.Domain.SportsData;
using BigBall.Shared.WorldCup;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Sync;

/// <summary>Maps authoritative <see cref="SportsMatchSnapshot"/> into persisted <see cref="Match"/> and refreshes <see cref="Team"/> badge rows.</summary>
internal static class SportsMatchFeedSyncApplier
{
    internal static async Task ApplyAsync(
        BigBallDbContext db,
        Match entity,
        SportsMatchSnapshot snap,
        DateTime syncedUtcUtc,
        DateTime utcNowUtc,
        CancellationToken ct)
    {
        entity.LastProviderStatusCode = snap.ProviderStatusCode;
        entity.LastLifecyclePhase = snap.Phase;
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

        await UpsertTeamBadgesAsync(db, entity.HomeCode, true, snap, utcNowUtc, ct).ConfigureAwait(false);
        await UpsertTeamBadgesAsync(db, entity.AwayCode, false, snap, utcNowUtc, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Insert-or-update a <see cref="Team"/> row from the snapshot's team-image fields.
    /// Never overwrites a cached URL with null (defensive against transient missing fields in the upstream payload).
    /// </summary>
    private static async Task UpsertTeamBadgesAsync(
        BigBallDbContext db,
        string code,
        bool home,
        SportsMatchSnapshot snap,
        DateTime utcNowUtc,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code))
            return;

        var url = home ? snap.HomeTeamImageUrl : snap.AwayTeamImageUrl;
        var urlSmall = home ? snap.HomeTeamImageUrlSmall : snap.AwayTeamImageUrlSmall;
        var fsId = home ? snap.HomeTeamFlashScoreId : snap.AwayTeamFlashScoreId;
        var fsUrl = home ? snap.HomeTeamFlashScoreUrl : snap.AwayTeamFlashScoreUrl;

        // If the snapshot has no badge info for this side, don't insert an empty row — leave whatever exists.
        if (string.IsNullOrEmpty(url) && string.IsNullOrEmpty(urlSmall)
            && string.IsNullOrEmpty(fsId) && string.IsNullOrEmpty(fsUrl))
            return;

        var team = await db.Teams.FirstOrDefaultAsync(t => t.Code == code, ct).ConfigureAwait(false);
        if (team is null)
        {
            team = new Team
            {
                Code = code,
                DisplayName = WorldCup2026TeamCodes.ToDisplayName(code),
            };
            db.Teams.Add(team);
        }

        if (!string.IsNullOrEmpty(url))
            team.BadgeUrl = url;
        if (!string.IsNullOrEmpty(urlSmall))
            team.BadgeUrlSmall = urlSmall;
        if (!string.IsNullOrEmpty(fsId))
            team.FlashScoreTeamId = fsId;
        if (!string.IsNullOrEmpty(fsUrl))
            team.FlashScoreTeamUrl = fsUrl;

        team.LastUpdatedUtc = utcNowUtc;
        team.LastSource = "match-sync";
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
