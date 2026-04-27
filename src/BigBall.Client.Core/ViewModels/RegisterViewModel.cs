using BigBall.Client.Core.Abstractions;
using BigBall.Client.Core.ViewModels.Base;
using BigBall.Shared.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BigBall.Client.Core.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{
    private readonly IAuthApi _auth;
    private readonly ITokenStore _tokens;
    private readonly IUserProfileStore _profileStore;
    private readonly IAppNavigator _navigator;

    public RegisterViewModel(IAuthApi auth, ITokenStore tokens, IUserProfileStore profileStore, IAppNavigator navigator)
    {
        _auth = auth;
        _tokens = tokens;
        _profileStore = profileStore;
        _navigator = navigator;
    }

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string? _successMessage;

    [ObservableProperty]
    private ProfileDto? _profile;

    [RelayCommand]
    private Task RegisterAsync(CancellationToken ct) => RunAsync(async token =>
    {
        SuccessMessage = null;
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Informe e-mail e senha.";
            return;
        }

        if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
        {
            ErrorMessage = "As senhas não coincidem.";
            return;
        }

        var redirect = _navigator.GetAuthEmailRedirectUrl();
        var response = await _auth.RegisterAsync(
            new RegisterRequest(
                Email.Trim(),
                Password,
                string.IsNullOrWhiteSpace(DisplayName) ? null : DisplayName.Trim(),
                redirect),
            token);

        if (response.Session is not null)
        {
            await _tokens.SetTokenAsync(response.Session.Token, token);
            await _profileStore.SetSnapshotAsync(response.Session.Profile, token);
            Profile = response.Session.Profile;
            _navigator.NavigateTo("/", replace: true);
            return;
        }

        if (response.RequiresEmailConfirmation)
        {
            ErrorMessage = null;
            var addr = response.PendingEmail ?? Email.Trim();
            SuccessMessage =
                $"Enviamos um link de confirmação para {addr}. Abra o e-mail para ativar sua conta e depois faça login.";
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }
    }, ct);
}
