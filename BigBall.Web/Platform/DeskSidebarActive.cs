namespace BigBall.Web.Platform;

/// <summary>
/// Maps the app-relative path to the <see cref="Shared.UI.Shell.Sidebar"/> <c>Active</c> segment.
/// </summary>
public static class DeskSidebarActive
{
    public static string FromPath(string relativePath)
    {
        var path = (relativePath ?? string.Empty).Trim('/');

        // Strip any query string or fragment so they don't get folded into the first segment.
        var cut = path.AsSpan().IndexOfAny('?', '#');
        if (cut >= 0)
        {
            path = path[..cut];
        }

        if (string.IsNullOrEmpty(path))
        {
            return "dashboard";
        }

        var first = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? string.Empty;

        return first switch
        {
            "calendar" => "calendar",
            "discover" => "discover",
            "profile" => "profile",
            "about" => "about",
            _ => "dashboard",
        };
    }
}
