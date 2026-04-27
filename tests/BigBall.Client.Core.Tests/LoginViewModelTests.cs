using BigBall.Client.Core.Abstractions;
using BigBall.Client.Core.ViewModels;
using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Tests;

public class LoginViewModelTests
{
    [Fact]
    public async Task LoginAsync_HappyPath_StoresTokenAndNavigatesHome()
    {
        var auth = new FakeAuthApi("stub-token");
        var tokens = new FakeTokenStore();
        var nav = new FakeNavigator();
        var vm = new LoginViewModel(auth, tokens, nav)
        {
            Email = "joao.pereira@gmail.com",
            Password = "x"
        };

        await vm.LoginCommand.ExecuteAsync(null);

        Assert.Equal("stub-token", tokens.StoredToken);
        Assert.Equal("/", nav.LastRoute);
        Assert.True(nav.LastReplace);
        Assert.Null(vm.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_EmptyInput_SetsErrorMessageAndSkipsApi()
    {
        var auth = new FakeAuthApi("unused");
        var tokens = new FakeTokenStore();
        var nav = new FakeNavigator();
        var vm = new LoginViewModel(auth, tokens, nav)
        {
            Email = "",
            Password = ""
        };

        await vm.LoginCommand.ExecuteAsync(null);

        Assert.Equal(0, auth.CallCount);
        Assert.Null(tokens.StoredToken);
        Assert.Null(nav.LastRoute);
        Assert.False(string.IsNullOrWhiteSpace(vm.ErrorMessage));
    }

    private sealed class FakeAuthApi : IAuthApi
    {
        private readonly string _token;
        public int CallCount { get; private set; }

        public FakeAuthApi(string token) => _token = token;

        public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            CallCount++;
            return Task.FromResult(new LoginResponse(
                _token,
                new ProfileDto(Guid.NewGuid(), "joao.pereira@gmail.com", "João Pereira", null, DateTime.UtcNow)));
        }

        public Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default) =>
            throw new NotImplementedException();

        public Task<OAuthRedirectUrlResponse> GetGoogleUrlAsync(string redirectTo, CancellationToken ct = default)
            => Task.FromResult(new OAuthRedirectUrlResponse("https://example.com/auth"));

        public Task<ProfileDto> GetMyProfileAsync(CancellationToken ct = default)
            => Task.FromResult(new ProfileDto(Guid.NewGuid(), "joao.pereira@gmail.com", "João Pereira", null, DateTime.UtcNow));
    }

    private sealed class FakeTokenStore : ITokenStore
    {
        public string? StoredToken { get; private set; }
        public ValueTask<string?> GetTokenAsync(CancellationToken ct = default) => ValueTask.FromResult(StoredToken);
        public ValueTask SetTokenAsync(string token, CancellationToken ct = default)
        {
            StoredToken = token;
            return ValueTask.CompletedTask;
        }
        public ValueTask ClearAsync(CancellationToken ct = default)
        {
            StoredToken = null;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeNavigator : IAppNavigator
    {
        public string? LastRoute { get; private set; }
        public bool LastReplace { get; private set; }
        public void NavigateTo(string route, bool replace = false) { LastRoute = route; LastReplace = replace; }
        public void NavigateToRoot() { LastRoute = "/"; LastReplace = true; }
    }
}
