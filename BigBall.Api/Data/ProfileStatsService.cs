using BigBall.Domain.Scoring;
using BigBall.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Data;

/// <summary>
/// Aggregates a user's prediction stats across ALL their pools for the profile dashboard.
/// Reuses <see cref="ScoringEngine"/> exactly like <see cref="RankingService"/>, but globally per user.
/// </summary>
public sealed class ProfileStatsService
{
    private const int RecentActivityCount = 8;

    private readonly BigBallDbContext _db;

    public ProfileStatsService(BigBallDbContext db)
    {
        _db = db;
    }

    public async Task<ProfileStatsDto> BuildAsync(Guid userId, CancellationToken ct = default)
    {
        var predictions = await _db.Predictions
            .Where(p => p.UserId == userId)
            .ToListAsync(ct);

        var eligibleCount = await _db.Matches.CountAsync(ct);
        var submittedCount = predictions.Select(p => p.MatchId).Distinct().Count();

        if (predictions.Count == 0)
        {
            return new ProfileStatsDto(0, eligibleCount, EmptyBands(), Array.Empty<ProfileActivityRowDto>());
        }

        var matchIds = predictions.Select(p => p.MatchId).Distinct().ToList();
        var poolIds = predictions.Select(p => p.PoolId).Distinct().ToList();

        var matches = await _db.Matches
            .Where(m => matchIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, ct);
        var poolNames = await _db.Pools
            .Where(p => poolIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, ct);

        var bandCounts = new Dictionary<int, int> { [20] = 0, [16] = 0, [15] = 0, [10] = 0, [5] = 0, [0] = 0 };
        var scored = new List<(Domain.Entities.Match Match, Domain.Entities.Prediction Prediction, int Points)>();

        foreach (var p in predictions)
        {
            if (!matches.TryGetValue(p.MatchId, out var match) || !match.HasReferenceScore)
            {
                continue;
            }

            var result = ScoringEngine.Score(
                p.Home, p.Away,
                match.ReferenceHome!.Value, match.ReferenceAway!.Value,
                p.PenaltyWinnerCode, match.PenaltyWinnerCode);

            bandCounts[result.Tier]++;
            scored.Add((match, p, result.Total));
        }

        var bands = new List<ScoringBandDto>
        {
            new(20, bandCounts[20]),
            new(16, bandCounts[16]),
            new(15, bandCounts[15]),
            new(10, bandCounts[10]),
            new(5, bandCounts[5]),
            new(0, bandCounts[0]),
        };

        var recent = scored
            .OrderByDescending(x => x.Match.KickoffUtc)
            .Take(RecentActivityCount)
            .Select(x => new ProfileActivityRowDto(
                x.Match.HomeCode,
                x.Match.AwayCode,
                poolNames.TryGetValue(x.Prediction.PoolId, out var name) ? name : "—",
                x.Match.ReferenceHome,
                x.Match.ReferenceAway,
                x.Prediction.Home,
                x.Prediction.Away,
                x.Points,
                x.Match.KickoffUtc))
            .ToList();

        return new ProfileStatsDto(submittedCount, eligibleCount, bands, recent);
    }

    private static IReadOnlyList<ScoringBandDto> EmptyBands() =>
    [
        new(20, 0), new(16, 0), new(15, 0), new(10, 0), new(5, 0), new(0, 0)
    ];
}
