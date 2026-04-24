using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Abstractions;

public interface IAuthApi
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
