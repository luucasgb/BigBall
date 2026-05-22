using BigBall.Domain.Enums;

namespace BigBall.Domain.Entities;

public sealed class Pool
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required PoolVisibility Visibility { get; set; }
    public string? InviteCode { get; set; }
    public required Guid AdminUserId { get; set; }
    public string? PrizeDescription { get; set; }
    public string? EntryCost { get; set; }
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
}
