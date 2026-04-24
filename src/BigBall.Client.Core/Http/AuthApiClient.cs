using System.Net.Http.Json;
using BigBall.Client.Core.Abstractions;
using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Http;

public sealed class AuthApiClient : IAuthApi
{
    private readonly HttpClient _http;

    public AuthApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/auth/login", request, HttpJsonExtensions.Json, ct).ConfigureAwait(false);
        return await response.ReadRequiredAsync<LoginResponse>(ct).ConfigureAwait(false);
    }
}
