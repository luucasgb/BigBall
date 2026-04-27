using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Abstractions;

public interface IAuthApi
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<OAuthRedirectUrlResponse> GetGoogleUrlAsync(string redirectTo, CancellationToken ct = default);
    Task<ProfileDto> GetMyProfileAsync(CancellationToken ct = default);
}
