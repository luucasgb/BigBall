using BigBall.Client.Core.Abstractions;
using BigBall.Client.Core.ViewModels;
using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Tests;

public class RegisterViewModelTests
{
    [Fact]
    public async Task RegisterAsync_WithSession_StoresTokenAndNavigatesHome()
    {
        var session = new LoginResponse(
            "stub-token",
            new ProfileDto(Guid.NewGuid(), "a@b.com", "A B", null, DateTime.UtcNow));
        var auth = new FakeAuthApi(registerReturns: new RegisterResponse(session, RequiresEmailConfirmation: false));
        var tokens = new FakeTokenStore();
        var nav = new FakeNavigator();
        var vm = new RegisterViewModel(auth, tokens, nav)
        {
            Email = "a@b.com",
            Password = "secret1",
            ConfirmPassword = "secret1"
        };

        await vm.RegisterCommand.ExecuteAsync(null);

        Assert.Equal("stub-token", tokens.StoredToken);
        Assert.Equal("/", nav.LastRoute);
        Assert.True(nav.LastReplace);
        Assert.Null(vm.ErrorMessage);
        Assert.Null(vm.SuccessMessage);
    }

    [Fact]
    public async Task RegisterAsync_EmailConfirmation_DoesNotStoreTokenAndSetsSuccessMessage()
    {
        var auth = new FakeAuthApi(registerReturns: new RegisterResponse(
            Session: null,
            RequiresEmailConfirmation: true,
            PendingEmail: "a@b.com"));
        var tokens = new FakeTokenStore();
        var nav = new FakeNavigator();
        var vm = new RegisterViewModel(auth, tokens, nav)
        {
            Email = "a@b.com",
            Password = "secret1",
            ConfirmPassword = "secret1"
        };

        await vm.RegisterCommand.ExecuteAsync(null);

        Assert.Null(tokens.StoredToken);
        Assert.Null(nav.LastRoute);
        Assert.Null(vm.ErrorMessage);
        Assert.NotNull(vm.SuccessMessage);
        Assert.Contains("a@b.com", vm.SuccessMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RegisterAsync_MismatchedPasswords_SkipsApi()
    {
        var auth = new FakeAuthApi(registerReturns: null);
        var tokens = new FakeTokenStore();
        var nav = new FakeNavigator();
        var vm = new RegisterViewModel(auth, tokens, nav)
        {
            Email = "a@b.com",
            Password = "a",
            ConfirmPassword = "b"
        };

        await vm.RegisterCommand.ExecuteAsync(null);

        Assert.Equal(0, auth.RegisterCallCount);
        Assert.Null(tokens.StoredToken);
        Assert.False(string.IsNullOrWhiteSpace(vm.ErrorMessage));
    }

    private sealed class FakeAuthApi : IAuthApi
    {
        private readonly LoginResponse _login;
        private readonly RegisterResponse? _registerReturns;

        public FakeAuthApi(LoginResponse? login = null, RegisterResponse? registerReturns = null)
        {
            _login = login ?? new LoginResponse("t", new ProfileDto(Guid.NewGuid(), "x@y.com", "X", null, DateTime.UtcNow));
            _registerReturns = registerReturns;
        }

        public int RegisterCallCount { get; private set; }

        public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default) =>
            Task.FromResult(_login);

        public Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        {
            RegisterCallCount++;
            return Task.FromResult(_registerReturns
                                   ?? throw new InvalidOperationException("Register not configured."));
        }

        public Task<OAuthRedirectUrlResponse> GetGoogleUrlAsync(string redirectTo, CancellationToken ct = default)
            => Task.FromResult(new OAuthRedirectUrlResponse("https://example.com/auth"));

        public Task<ProfileDto> GetMyProfileAsync(CancellationToken ct = default) =>
            Task.FromResult(new ProfileDto(Guid.NewGuid(), "x@y.com", "X", null, DateTime.UtcNow));
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
