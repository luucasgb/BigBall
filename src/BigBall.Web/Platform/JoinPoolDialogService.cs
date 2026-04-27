namespace BigBall.Web.Platform;

public interface IJoinPoolDialogService
{
    bool IsOpen { get; }
    void Open();
    void Close();
    event Action? StateChanged;
}

public sealed class JoinPoolDialogService : IJoinPoolDialogService
{
    public bool IsOpen { get; private set; }

    public event Action? StateChanged;

    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;
        StateChanged?.Invoke();
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        StateChanged?.Invoke();
    }
}
