namespace BigBall.Client.Core.Abstractions;

/// <summary>
/// Clears persisted credentials and leaves the authenticated area of the app.
/// </summary>
public interface IAuthSession
{
    ValueTask LogoutAsync(CancellationToken cancellationToken = default);
}
