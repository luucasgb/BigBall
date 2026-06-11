using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Abstractions;

public interface IMatchesApi
{
    Task<IReadOnlyList<MatchCalendarRowDto>> GetMatchesInRangeAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct = default);

    Task<MatchDetailDto> GetMatchAsync(Guid matchId, Guid poolId, CancellationToken ct = default);

    Task<IReadOnlyList<PoolPredictionDto>> GetMyPoolPredictionsAsync(Guid matchId, CancellationToken ct = default);
}
