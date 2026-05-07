namespace BigBall.Web.Platform;

public interface ICreatePoolDialogService
{
    bool IsOpen { get; }
    void Open();
    void Close();
    event Action? StateChanged;
}

public sealed class CreatePoolDialogService : ICreatePoolDialogService
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
