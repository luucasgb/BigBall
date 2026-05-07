using BigBall.Client.Core.Abstractions;

namespace BigBall.Web.Platform;

public sealed class WebAuthSession : IAuthSession
{
    private readonly ITokenStore _tokens;
    private readonly IUserProfileStore _profileStore;
    private readonly IAppNavigator _navigator;

    public WebAuthSession(ITokenStore tokens, IUserProfileStore profileStore, IAppNavigator navigator)
    {
        _tokens = tokens;
        _profileStore = profileStore;
        _navigator = navigator;
    }

    public async ValueTask LogoutAsync(CancellationToken cancellationToken = default)
    {
        await _tokens.ClearAsync(cancellationToken);
        await _profileStore.ClearAsync(cancellationToken);
        _navigator.NavigateTo("/login", replace: true);
    }
}
