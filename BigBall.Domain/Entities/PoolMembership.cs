using BigBall.Domain.Enums;

namespace BigBall.Domain.Entities;

public sealed class PoolMembership
{
    public required Guid Id { get; init; }
    public required Guid PoolId { get; init; }
    public required Guid UserId { get; init; }
    public required MembershipRole Role { get; set; }
    public required DateTime JoinedUtc { get; init; }
}
