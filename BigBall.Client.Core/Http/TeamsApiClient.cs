using BigBall.Client.Core.Abstractions;
using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Http;

public sealed class TeamsApiClient : ITeamsApi
{
    private readonly HttpClient _http;

    public TeamsApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<TeamDto>> GetTeamsAsync(CancellationToken ct = default)
    {
        using var response = await _http.GetAsync("api/teams", ct).ConfigureAwait(false);
        return await response.ReadRequiredAsync<List<TeamDto>>(ct).ConfigureAwait(false);
    }
}
