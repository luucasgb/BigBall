using System.Net.Http.Json;
using BigBall.Client.Core.Abstractions;
using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Http;

public sealed class AdminApiClient : IAdminApi
{
    private readonly HttpClient _http;

    public AdminApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<WorldCup2026SportsApiProbeDto> ProbeWorldCup2026SportsApiAsync(CancellationToken ct = default)
    {
        using var response = await _http
            .PostAsync("api/admin/debug/world-cup-2026-sports-api-probe", null, ct)
            .ConfigureAwait(false);

        var dto = await response.Content
            .ReadFromJsonAsync<WorldCup2026SportsApiProbeDto>(HttpJsonExtensions.Json, ct)
            .ConfigureAwait(false);

        if (dto is null)
        {
            throw new InvalidOperationException("Probe returned an empty body.");
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException("Sign in required for admin probe.");
        }

        return dto;
    }
}
