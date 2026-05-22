using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Abstractions;

public interface IAdminApi
{
    Task<WorldCup2026SportsApiProbeDto> ProbeWorldCup2026SportsApiAsync(CancellationToken ct = default);
}
