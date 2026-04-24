namespace BigBall.Client.Core.Abstractions;

/// <summary>
/// Platform-specific token persistence. Blazor implements via localStorage; MAUI via SecureStorage.
/// </summary>
public interface ITokenStore
{
    ValueTask<string?> GetTokenAsync(CancellationToken ct = default);
    ValueTask SetTokenAsync(string token, CancellationToken ct = default);
    ValueTask ClearAsync(CancellationToken ct = default);
}
