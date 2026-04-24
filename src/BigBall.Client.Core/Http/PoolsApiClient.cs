using System.Net.Http.Json;
using BigBall.Client.Core.Abstractions;
using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Http;

public sealed class PoolsApiClient : IPoolsApi
{
    private readonly HttpClient _http;

    public PoolsApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<MyPoolDto>> GetMyPoolsAsync(CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<List<MyPoolDto>>("api/pools/mine", HttpJsonExtensions.Json, ct).ConfigureAwait(false);
        return result ?? new List<MyPoolDto>();
    }

    public async Task<PoolDetailDto> GetPoolAsync(Guid poolId, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync($"api/pools/{poolId}", ct).ConfigureAwait(false);
        return await response.ReadRequiredAsync<PoolDetailDto>(ct).ConfigureAwait(false);
    }
}
