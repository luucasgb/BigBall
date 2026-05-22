using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Abstractions;

public interface ITeamsApi
{
    Task<IReadOnlyList<TeamDto>> GetTeamsAsync(CancellationToken ct = default);
}
