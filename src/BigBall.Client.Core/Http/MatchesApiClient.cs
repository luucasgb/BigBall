using BigBall.Client.Core.Abstractions;
using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Http;

public sealed class MatchesApiClient : IMatchesApi
{
    private readonly HttpClient _http;

    public MatchesApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<MatchDetailDto> GetMatchAsync(Guid matchId, Guid poolId, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync($"api/matches/{matchId}?poolId={poolId}", ct).ConfigureAwait(false);
        return await response.ReadRequiredAsync<MatchDetailDto>(ct).ConfigureAwait(false);
    }
}
