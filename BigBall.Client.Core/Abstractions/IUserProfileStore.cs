using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Abstractions;

/// <summary>
/// Client-side snapshot of the signed-in user's profile (aligned with <c>api/auth/me</c> / <see cref="ProfileDto"/>).
/// </summary>
public interface IUserProfileStore
{
    ProfileDto? Snapshot { get; }

    event Action? SnapshotChanged;

    ValueTask SetSnapshotAsync(ProfileDto profile, CancellationToken ct = default);

    ValueTask ClearAsync(CancellationToken ct = default);

    /// <summary>
    /// If <see cref="Snapshot"/> is null, restores it from persisted storage (when available).
    /// </summary>
    ValueTask EnsureHydratedFromStorageAsync(CancellationToken ct = default);
}
