using BigBall.Client.Core.Abstractions;
using Microsoft.JSInterop;

namespace BigBall.Web.Platform;

/// <summary>
/// STUB — substituir por Supabase JWT conforme TechSpec §4.3.
/// Persists the JWT in window.localStorage. Future MAUI implementation uses SecureStorage.
/// </summary>
public sealed class LocalStorageTokenStore : ITokenStore
{
    private const string Key = "bigball.auth.token";
    private readonly IJSRuntime _js;

    public LocalStorageTokenStore(IJSRuntime js) => _js = js;

    public async ValueTask<string?> GetTokenAsync(CancellationToken ct = default)
    {
        try
        {
            return await _js.InvokeAsync<string?>("localStorage.getItem", ct, Key);
        }
        catch (InvalidOperationException)
        {
            // Prerender phase: JS isn't available yet.
            return null;
        }
    }

    public async ValueTask SetTokenAsync(string token, CancellationToken ct = default) =>
        await _js.InvokeVoidAsync("localStorage.setItem", ct, Key, token);

    public async ValueTask ClearAsync(CancellationToken ct = default) =>
        await _js.InvokeVoidAsync("localStorage.removeItem", ct, Key);
}
