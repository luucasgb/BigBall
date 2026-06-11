namespace BigBall.Client.Core.Abstractions;

/// <summary>
/// Thin navigation abstraction so ViewModels don't depend on Blazor's NavigationManager
/// or MAUI's Shell.Navigation directly.
/// </summary>
public interface IAppNavigator
{
    void NavigateTo(string route, bool replace = false);
    void NavigateToRoot();

    /// <summary>
    /// Absolute URL for Supabase e-mail actions (signup confirmation). Null skips <c>redirect_to</c> on the API request.
    /// </summary>
    string? GetAuthEmailRedirectUrl() => null;
}
