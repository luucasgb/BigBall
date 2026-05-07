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

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("api/auth/register", request, HttpJsonExtensions.Json, ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var msg = await response.TryReadErrorMessageAsync(ct).ConfigureAwait(false);
            throw new InvalidOperationException(msg ?? $"Cadastro falhou ({(int)response.StatusCode}).");
        }

        var body = await response.Content.ReadFromJsonAsync<RegisterResponse>(HttpJsonExtensions.Json, ct).ConfigureAwait(false);
        return body ?? throw new InvalidOperationException("API retornou resposta vazia no cadastro.");
    }

    public async Task<OAuthRedirectUrlResponse> GetGoogleUrlAsync(string redirectTo, CancellationToken ct = default)
    {
        var endpoint = $"api/auth/google-url?redirectTo={Uri.EscapeDataString(redirectTo)}";
        using var response = await _http.GetAsync(endpoint, ct).ConfigureAwait(false);
        return await response.ReadRequiredAsync<OAuthRedirectUrlResponse>(ct).ConfigureAwait(false);
    }

    public async Task<ProfileDto> GetMyProfileAsync(CancellationToken ct = default)
    {
        using var response = await _http.GetAsync("api/auth/me", ct).ConfigureAwait(false);
        return await response.ReadRequiredAsync<ProfileDto>(ct).ConfigureAwait(false);
    }
}
