using System.Text.Json;
using System.Text.Json.Serialization;
using BigBall.Client.Core.Abstractions;
using BigBall.Shared.Dtos;
using Microsoft.JSInterop;

namespace BigBall.Web.Platform;

public sealed class LocalUserProfileStore : IUserProfileStore
{
    private const string Key = "bigball.auth.profile";
    private readonly IJSRuntime _js;
    private ProfileDto? _snapshot;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public LocalUserProfileStore(IJSRuntime js) => _js = js;

    public ProfileDto? Snapshot => _snapshot;

    public event Action? SnapshotChanged;

    public async ValueTask SetSnapshotAsync(ProfileDto profile, CancellationToken ct = default)
    {
        _snapshot = profile;
        SnapshotChanged?.Invoke();
        try
        {
            var json = JsonSerializer.Serialize(profile, JsonOptions);
            await _js.InvokeVoidAsync("localStorage.setItem", ct, Key, json).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            // Prerender: JS not available.
        }
    }

    public async ValueTask ClearAsync(CancellationToken ct = default)
    {
        _snapshot = null;
        SnapshotChanged?.Invoke();
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", ct, Key).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
        }
    }

    public async ValueTask EnsureHydratedFromStorageAsync(CancellationToken ct = default)
    {
        if (_snapshot is not null)
        {
            return;
        }

        try
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", ct, Key).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            var dto = JsonSerializer.Deserialize<ProfileDto>(json, JsonOptions);
            if (dto is null)
            {
                return;
            }

            _snapshot = dto;
            SnapshotChanged?.Invoke();
        }
        catch (InvalidOperationException)
        {
        }
    }
}
