using BigBall.Client.Core.Abstractions;
using BigBall.Shared.Dtos;

namespace BigBall.Web.Shared.State;

/// <summary>
/// Per-session lazy cache of <c>GET /api/teams</c>. One HTTP call per Blazor WASM session;
/// pages call <see cref="EnsureLoadedAsync"/> on init and read URLs via the synchronous helpers.
/// </summary>
public sealed class TeamBadgeCache
{
    private readonly ITeamsApi _api;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private Dictionary<string, TeamDto> _byCode = new(StringComparer.OrdinalIgnoreCase);
    private bool _loaded;

    public TeamBadgeCache(ITeamsApi api)
    {
        _api = api;
    }

    public async Task EnsureLoadedAsync(CancellationToken ct = default)
    {
        if (_loaded)
            return;

        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_loaded)
                return;

            try
            {
                var list = await _api.GetTeamsAsync(ct).ConfigureAwait(false);
                _byCode = list.ToDictionary(t => t.Code, StringComparer.OrdinalIgnoreCase);
                _loaded = true;
            }
            catch (Exception)
            {
                // Network/API failure: leave the cache empty and DO NOT mark loaded —
                // the next component init retries instead of silently locking us into
                // the SVG-gradient fallback for the rest of the session.
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public string? GetBadgeUrl(string? code)
        => code is not null && _byCode.TryGetValue(code, out var t) ? t.BadgeUrl : null;

    public string? GetBadgeUrlSmall(string? code)
        => code is not null && _byCode.TryGetValue(code, out var t) ? t.BadgeUrlSmall : null;
}
