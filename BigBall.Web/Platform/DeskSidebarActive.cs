namespace BigBall.Web.Platform;

/// <summary>
/// Maps the app-relative path to the <see cref="Shared.UI.Shell.Sidebar"/> <c>Active</c> segment.
/// </summary>
public static class DeskSidebarActive
{
    public static string FromPath(string relativePath)
    {
        var path = (relativePath ?? string.Empty).Trim('/');
        if (string.IsNullOrEmpty(path))
        {
            return "dashboard";
        }

        var first = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? string.Empty;

        return first switch
        {
            "calendar" => "calendar",
            "profile" => "profile",
            "about" => "about",
            _ => "dashboard",
        };
    }
}
