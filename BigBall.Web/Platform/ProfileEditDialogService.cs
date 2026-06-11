namespace BigBall.Web.Platform;

/// <summary>Qual campo do perfil o modal de edição deve mostrar.</summary>
public enum ProfileEditField
{
    DisplayName,
    TimeZone
}

public interface IProfileEditDialogService
{
    bool IsOpen { get; }
    ProfileEditField Field { get; }
    void Open(ProfileEditField field);
    void Close();
    event Action? StateChanged;
}

public sealed class ProfileEditDialogService : IProfileEditDialogService
{
    public bool IsOpen { get; private set; }
    public ProfileEditField Field { get; private set; }

    public event Action? StateChanged;

    public void Open(ProfileEditField field)
    {
        Field = field;
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
