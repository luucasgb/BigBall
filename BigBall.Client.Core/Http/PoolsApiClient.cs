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

    public async Task<IReadOnlyList<PoolMatchRowDto>> GetPoolMatchesAsync(Guid poolId, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync($"api/pools/{poolId}/matches", ct).ConfigureAwait(false);
        return await response.ReadRequiredAsync<List<PoolMatchRowDto>>(ct).ConfigureAwait(false);
    }

    public async Task<CreatePoolResponse> CreatePoolAsync(CreatePoolRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/pools", request, HttpJsonExtensions.Json, ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var msg = await response.TryReadErrorMessageAsync(ct).ConfigureAwait(false);
            throw new InvalidOperationException(msg ?? $"Não foi possível criar o bolão ({(int)response.StatusCode}).");
        }

        var body = await response.Content.ReadFromJsonAsync<CreatePoolResponse>(HttpJsonExtensions.Json, ct).ConfigureAwait(false);
        return body ?? throw new InvalidOperationException("API retornou resposta vazia ao criar bolão.");
    }

    public async Task<JoinPoolResponse> JoinPoolByInviteAsync(JoinPoolRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/pools/join", request, HttpJsonExtensions.Json, ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var msg = await response.TryReadErrorMessageAsync(ct).ConfigureAwait(false);
            throw new InvalidOperationException(msg ?? $"Não foi possível entrar no bolão ({(int)response.StatusCode}).");
        }

        var body = await response.Content.ReadFromJsonAsync<JoinPoolResponse>(HttpJsonExtensions.Json, ct).ConfigureAwait(false);
        return body ?? throw new InvalidOperationException("API retornou resposta vazia ao entrar no bolão.");
    }

    public async Task<IReadOnlyList<PublicPoolDto>> GetPublicPoolsAsync(CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<List<PublicPoolDto>>("api/pools/public", HttpJsonExtensions.Json, ct).ConfigureAwait(false);
        return result ?? new List<PublicPoolDto>();
    }

    public async Task<JoinPoolResponse> JoinPublicPoolAsync(Guid poolId, CancellationToken ct = default)
    {
        using var response = await _http.PostAsync($"api/pools/{poolId}/join", content: null, ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var msg = await response.TryReadErrorMessageAsync(ct).ConfigureAwait(false);
            throw new InvalidOperationException(msg ?? $"Não foi possível entrar no bolão ({(int)response.StatusCode}).");
        }

        var body = await response.Content.ReadFromJsonAsync<JoinPoolResponse>(HttpJsonExtensions.Json, ct).ConfigureAwait(false);
        return body ?? throw new InvalidOperationException("API retornou resposta vazia ao entrar no bolão.");
    }
}
