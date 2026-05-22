using BigBall.Web.Platform;

namespace BigBall.Web.Tests;

public class CreatePoolDialogServiceTests
{
    [Fact]
    public void Open_SetsIsOpen_AndInvokesStateChanged()
    {
        var sut = new CreatePoolDialogService();
        var count = 0;
        sut.StateChanged += () => count++;

        sut.Open();

        Assert.True(sut.IsOpen);
        Assert.Equal(1, count);
    }

    [Fact]
    public void Open_WhenAlreadyOpen_IsNoop()
    {
        var sut = new CreatePoolDialogService();
        var count = 0;
        sut.StateChanged += () => count++;

        sut.Open();
        sut.Open();

        Assert.True(sut.IsOpen);
        Assert.Equal(1, count);
    }

    [Fact]
    public void Close_ClearsIsOpen_AndInvokesStateChanged()
    {
        var sut = new CreatePoolDialogService();
        var count = 0;
        sut.StateChanged += () => count++;
        sut.Open();
        var afterOpen = count;

        sut.Close();

        Assert.False(sut.IsOpen);
        Assert.Equal(afterOpen + 1, count);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_IsNoop()
    {
        var sut = new CreatePoolDialogService();
        var count = 0;
        sut.StateChanged += () => count++;

        sut.Close();
        sut.Close();

        Assert.False(sut.IsOpen);
        Assert.Equal(0, count);
    }

    [Fact]
    public void Open_Close_Open_Sequence_NotifiesEachTransition()
    {
        var sut = new CreatePoolDialogService();
        var events = new List<string>();
        sut.StateChanged += () => events.Add(sut.IsOpen ? "open" : "closed");

        sut.Open();
        sut.Close();
        sut.Open();

        Assert.Equal(new[] { "open", "closed", "open" }, events);
    }
}
