using BigBall.Web.Platform;

namespace BigBall.Web.Tests;

public class DeskSidebarActiveTests
{
    [Theory]
    [InlineData("", "dashboard")]
    [InlineData("/", "dashboard")]
    [InlineData("calendar", "calendar")]
    [InlineData("profile", "profile")]
    [InlineData("predict", "predict")]
    [InlineData("pools/abc", "dashboard")]
    [InlineData("pools/abc/predict/def", "predict")]
    public void FromPath_MapsRelativePath(string relative, string expected) =>
        Assert.Equal(expected, DeskSidebarActive.FromPath(relative));

    [Fact]
    public void FromPath_TrimsSlashes() =>
        Assert.Equal("calendar", DeskSidebarActive.FromPath("/calendar/"));
}
