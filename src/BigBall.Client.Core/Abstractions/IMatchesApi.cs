using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Abstractions;

public interface IMatchesApi
{
    Task<MatchDetailDto> GetMatchAsync(Guid matchId, Guid poolId, CancellationToken ct = default);
}
