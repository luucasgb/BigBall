using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Abstractions;

public interface IPoolsApi
{
    Task<IReadOnlyList<MyPoolDto>> GetMyPoolsAsync(CancellationToken ct = default);
    Task<PoolDetailDto> GetPoolAsync(Guid poolId, CancellationToken ct = default);
}
