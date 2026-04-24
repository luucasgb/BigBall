using BigBall.Client.Core.Abstractions;
using Microsoft.AspNetCore.Components;

namespace BigBall.Web.Platform;

public sealed class BlazorAppNavigator : IAppNavigator
{
    private readonly NavigationManager _nav;

    public BlazorAppNavigator(NavigationManager nav) => _nav = nav;

    public void NavigateTo(string route, bool replace = false) =>
        _nav.NavigateTo(route, forceLoad: false, replace: replace);

    public void NavigateToRoot() => _nav.NavigateTo("/", forceLoad: false, replace: true);
}
