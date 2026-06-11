using BigBall.Domain.Scoring;
using BigBall.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Data;

/// <summary>
/// Computes per-pool rankings from Postgres.
/// Applies the partial PRD 4.8 chain: total desc → tier20 count → tier16 count → bonus count.
/// Full alphabetical + neutral-draw sortition is explicitly out of scope for the stub (PRD 4.8 end).
/// Rows sharing all the above counters get a shared TieGroupId so the UI can flag them.
/// </summary>
public sealed class RankingService
{
    private readonly BigBallDbContext _db;

    public RankingService(BigBallDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RankingRowDto>> BuildRankingAsync(Guid poolId, Guid currentUserId, CancellationToken ct = default)
    {
        var memberIds = await _db.PoolMemberships
            .Where(m => m.PoolId == poolId)
            .Select(m => m.UserId)
            .ToListAsync(ct);

        // Sequential: one DbContext cannot run overlapping queries (Task.WhenAll caused 500s).
        var rowsRaw = new List<(Guid UserId, string Name, int Points, int Tier20, int Tier16, int Tier15, int Tier10, int Tier5, int Bonus, int Trend, int PredictedGames, bool IsMe)>(memberIds.Count);
        foreach (var userId in memberIds)
        {
            rowsRaw.Add(await BuildRowAsync(poolId, userId, currentUserId, ct));
        }

        var ordered = rowsRaw
            .OrderByDescending(r => r.Points)
            .ThenByDescending(r => r.Tier20)
            .ThenByDescending(r => r.Tier16)
            .ThenByDescending(r => r.Bonus)
            .ToList();

        return AssignRanksAndTies(ordered);
    }

    private async Task<(Guid UserId, string Name, int Points, int Tier20, int Tier16, int Tier15, int Tier10, int Tier5, int Bonus, int Trend, int PredictedGames, bool IsMe)> BuildRowAsync(
        Guid poolId, Guid userId, Guid currentUserId, CancellationToken ct)
    {
        var profile = await _db.Profiles.FirstAsync(p => p.Id == userId, ct);
        int total = 0, t20 = 0, t16 = 0, t15 = 0, t10 = 0, t5 = 0, bonus = 0, trend = 0;

        var predictions = await _db.Predictions
            .Where(p => p.UserId == userId && p.PoolId == poolId)
            .ToListAsync(ct);
        int predictedGames = predictions.Count;

        DateTime? lastKickoff = null;
        foreach (var p in predictions)
        {
            var match = await _db.Matches.FirstOrDefaultAsync(m => m.Id == p.MatchId, ct);
            if (match is null) continue;
            if (!match.HasReferenceScore) continue;
            var result = ScoringEngine.Score(
                p.Home, p.Away,
                match.ReferenceHome!.Value, match.ReferenceAway!.Value,
                p.PenaltyWinnerCode, match.PenaltyWinnerCode);
            total += result.Total;
            if (result.Tier == 20) t20++;
            if (result.Tier == 16) t16++;
            if (result.Tier == 15) t15++;
            if (result.Tier == 10) t10++;
            if (result.Tier == 5) t5++;
            if (result.Bonus > 0) bonus++;
            if (lastKickoff is null || match.KickoffUtc > lastKickoff.Value)
            {
                lastKickoff = match.KickoffUtc;
                trend = result.Total;
            }
        }

        return (userId, profile.DisplayName, total, t20, t16, t15, t10, t5, bonus, trend, predictedGames, userId == currentUserId);
    }

    private static IReadOnlyList<RankingRowDto> AssignRanksAndTies(
        List<(Guid UserId, string Name, int Points, int Tier20, int Tier16, int Tier15, int Tier10, int Tier5, int Bonus, int Trend, int PredictedGames, bool IsMe)> ordered)
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
                row.Tier15,
                row.Tier10,
                row.Tier5,
                row.Bonus,
                row.Trend,
                row.PredictedGames,
                row.IsMe,
                tieGroupId));
        }
        return result;
    }

    private static bool AreTiedOnAllCounters(
        (Guid UserId, string Name, int Points, int Tier20, int Tier16, int Tier15, int Tier10, int Tier5, int Bonus, int Trend, int PredictedGames, bool IsMe) a,
        (Guid UserId, string Name, int Points, int Tier20, int Tier16, int Tier15, int Tier10, int Tier5, int Bonus, int Trend, int PredictedGames, bool IsMe) b)
        => a.Points == b.Points
           && a.Tier20 == b.Tier20
           && a.Tier16 == b.Tier16
           && a.Bonus == b.Bonus;
}
