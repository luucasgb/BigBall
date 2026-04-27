using BigBall.Client.Core.Abstractions;
using BigBall.Client.Core.ViewModels.Base;
using BigBall.Shared.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BigBall.Client.Core.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthApi _auth;
    private readonly ITokenStore _tokens;
    private readonly IUserProfileStore _profileStore;
    private readonly IAppNavigator _navigator;

    public LoginViewModel(IAuthApi auth, ITokenStore tokens, IUserProfileStore profileStore, IAppNavigator navigator)
    {
        _auth = auth;
        _tokens = tokens;
        _profileStore = profileStore;
        _navigator = navigator;
    }

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private ProfileDto? _profile;

    [ObservableProperty]
    private string? _googleOAuthUrl;

    [RelayCommand]
    private Task LoginAsync(CancellationToken ct) => RunAsync(async token =>
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Informe e-mail e senha.";
            return;
        }
        var response = await _auth.LoginAsync(new LoginRequest(Email.Trim(), Password), token);
        await _tokens.SetTokenAsync(response.Token, token);
        await _profileStore.SetSnapshotAsync(response.Profile, token);
        Profile = response.Profile;
        _navigator.NavigateTo("/", replace: true);
    }, ct);

    [RelayCommand]
    private Task StartGoogleLoginAsync(string redirectTo, CancellationToken ct) => RunAsync(async token =>
    {
        var response = await _auth.GetGoogleUrlAsync(redirectTo, token);
        GoogleOAuthUrl = response.Url;
    }, ct);
}
