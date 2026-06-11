using System.Net;
using System.Net.Http.Json;
using BigBall.Client.Core.Abstractions;
using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Http;

public sealed class PredictionsApiClient : IPredictionsApi
{
    private readonly HttpClient _http;

    public PredictionsApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<PredictionDto> UpsertAsync(
        Guid poolId,
        Guid matchId,
        UpsertPredictionRequest request,
        CancellationToken ct = default)
    {
        using var response = await _http.PutAsJsonAsync(
            $"api/pools/{poolId}/matches/{matchId}/prediction",
            request,
            HttpJsonExtensions.Json,
            ct).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var err = await response.Content.ReadFromJsonAsync<LockedError>(HttpJsonExtensions.Json, ct).ConfigureAwait(false)
                      ?? LockedError.Default;
            throw new PredictionLockedException(err.Message);
        }

        return await response.ReadRequiredAsync<PredictionDto>(ct).ConfigureAwait(false);
    }
}
