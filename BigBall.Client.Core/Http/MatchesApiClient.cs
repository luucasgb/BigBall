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

    public async Task<IReadOnlyList<MatchCalendarRowDto>> GetMatchesInRangeAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken ct = default)
    {
        var from = Uri.EscapeDataString(NormalizeUtc(fromUtc).ToString("O"));
        var to = Uri.EscapeDataString(NormalizeUtc(toUtc).ToString("O"));
        using var response = await _http.GetAsync($"api/matches?fromUtc={from}&toUtc={to}", ct).ConfigureAwait(false);
        return await response.ReadRequiredAsync<List<MatchCalendarRowDto>>(ct).ConfigureAwait(false);
    }

    public async Task<MatchDetailDto> GetMatchAsync(Guid matchId, Guid poolId, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync($"api/matches/{matchId}?poolId={poolId}", ct).ConfigureAwait(false);
        return await response.ReadRequiredAsync<MatchDetailDto>(ct).ConfigureAwait(false);
    }

    private static DateTime NormalizeUtc(DateTime t) =>
        t.Kind == DateTimeKind.Utc ? t : DateTime.SpecifyKind(t.ToUniversalTime(), DateTimeKind.Utc);
}
