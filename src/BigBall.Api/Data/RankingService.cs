using BigBall.Domain.Entities;
using BigBall.Domain.Scoring;
using BigBall.Shared.Dtos;

namespace BigBall.Api.Data;

/// <summary>
/// Computes per-pool rankings from the in-memory store.
/// Applies the partial PRD 4.8 chain: total desc → tier20 count → tier16 count → bonus count.
/// Full alphabetical + neutral-draw sortition is explicitly out of scope for the stub (PRD 4.8 end).
/// Rows sharing all the above counters get a shared TieGroupId so the UI can flag them.
/// </summary>
public sealed class RankingService
{
    private readonly InMemoryStore _store;

    public RankingService(InMemoryStore store)
    {
        _store = store;
    }

    public IReadOnlyList<RankingRowDto> BuildRanking(Guid poolId, Guid currentUserId)
    {
        _store.RankingLock.EnterReadLock();
        try
        {
            var memberIds = _store.MembersOf(poolId).Select(m => m.UserId).ToList();
            var rowsRaw = memberIds.Select(userId => BuildRow(poolId, userId, currentUserId)).ToList();

            var ordered = rowsRaw
                .OrderByDescending(r => r.Points)
                .ThenByDescending(r => r.Tier20)
                .ThenByDescending(r => r.Tier16)
                .ThenByDescending(r => r.Bonus)
                .ToList();

            return AssignRanksAndTies(ordered);
        }
        finally
        {
            _store.RankingLock.ExitReadLock();
        }
    }

    private (Guid UserId, string Name, int Points, int Tier20, int Tier16, int Bonus, int Trend, bool IsMe) BuildRow(
        Guid poolId, Guid userId, Guid currentUserId)
    {
        var profile = _store.Profiles[userId];
        int total = 0, t20 = 0, t16 = 0, bonus = 0, trend = 0;

        var predictions = _store.Predictions.Values
            .Where(p => p.UserId == userId && p.PoolId == poolId)
            .ToList();

        Match? lastFinished = null;
        foreach (var p in predictions)
        {
            if (!_store.Matches.TryGetValue(p.MatchId, out var match)) continue;
            if (!match.HasReferenceScore) continue;
            var result = ScoringEngine.Score(
                p.Home, p.Away,
                match.ReferenceHome!.Value, match.ReferenceAway!.Value,
                p.PenaltyWinnerCode, match.PenaltyWinnerCode);
            total += result.Total;
            if (result.Tier == 20) t20++;
            if (result.Tier == 16) t16++;
            if (result.Bonus > 0) bonus++;
            if (lastFinished is null || match.KickoffUtc > lastFinished.KickoffUtc)
            {
                lastFinished = match;
                trend = result.Total;
            }
        }

        return (userId, profile.DisplayName, total, t20, t16, bonus, trend, userId == currentUserId);
    }

    private static IReadOnlyList<RankingRowDto> AssignRanksAndTies(
        List<(Guid UserId, string Name, int Points, int Tier20, int Tier16, int Bonus, int Trend, bool IsMe)> ordered)
    {
        var result = new List<RankingRowDto>(ordered.Count);
        int currentTieGroup = 0;
        for (int i = 0; i < ordered.Count; i++)
        {
            int rank = i + 1;
            int? tieGroupId = null;

            bool sameAsPrev = i > 0 && AreTiedOnAllCounters(ordered[i], ordered[i - 1]);
            bool sameAsNext = i < ordered.Count - 1 && AreTiedOnAllCounters(ordered[i], ordered[i + 1]);

            if (sameAsPrev || sameAsNext)
            {
                if (!sameAsPrev) currentTieGroup++;
                tieGroupId = currentTieGroup;
                if (sameAsPrev)
                {
                    rank = result[i - 1].Rank;
                }
            }

            var row = ordered[i];
            result.Add(new RankingRowDto(
                rank,
                row.UserId,
                row.Name,
                row.Points,
                row.Tier20,
                row.Tier16,
                row.Bonus,
                row.Trend,
                row.IsMe,
                tieGroupId));
        }
        return result;
    }

    private static bool AreTiedOnAllCounters(
        (Guid UserId, string Name, int Points, int Tier20, int Tier16, int Bonus, int Trend, bool IsMe) a,
        (Guid UserId, string Name, int Points, int Tier20, int Tier16, int Bonus, int Trend, bool IsMe) b)
        => a.Points == b.Points
           && a.Tier20 == b.Tier20
           && a.Tier16 == b.Tier16
           && a.Bonus == b.Bonus;
}
