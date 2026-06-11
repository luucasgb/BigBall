using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Abstractions;

public interface IPoolsApi
{
    Task<IReadOnlyList<MyPoolDto>> GetMyPoolsAsync(CancellationToken ct = default);
    Task<PoolDetailDto> GetPoolAsync(Guid poolId, CancellationToken ct = default);
    Task<IReadOnlyList<PoolMatchRowDto>> GetPoolMatchesAsync(Guid poolId, CancellationToken ct = default);
    Task<CreatePoolResponse> CreatePoolAsync(CreatePoolRequest request, CancellationToken ct = default);
    Task<JoinPoolResponse> JoinPoolByInviteAsync(JoinPoolRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PublicPoolDto>> GetPublicPoolsAsync(CancellationToken ct = default);
    Task<JoinPoolResponse> JoinPublicPoolAsync(Guid poolId, CancellationToken ct = default);
}
