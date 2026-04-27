using BigBall.Domain.Enums;

namespace BigBall.Domain.Entities;

public sealed class Match
{
    public required Guid Id { get; init; }
    public required MatchPhase Phase { get; set; }
    public string? GroupLabel { get; set; }
    public required string HomeCode { get; set; }
    public required string AwayCode { get; set; }

    /// <summary>Idempotent import key, e.g. <c>wc2026-79</c> or <c>wc2026-…</c> for group games.</summary>
    public string? ExternalKey { get; set; }

    public required DateTime KickoffUtc { get; set; }
    public string? Venue { get; set; }
    public int? HostCityId { get; set; }
    public HostCity? HostCity { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;

    public int? ReferenceHome { get; set; }
    public int? ReferenceAway { get; set; }

    public bool WentToPenalties { get; set; }
    public string? PenaltyWinnerCode { get; set; }

    /// <summary>Instant when predictions close: same as <see cref="KickoffUtc"/> (official start).</summary>
    public DateTime LockUtc => KickoffUtc;

    public bool IsLocked(DateTime nowUtc) => nowUtc >= KickoffUtc;
    public bool HasReferenceScore => ReferenceHome is not null && ReferenceAway is not null;
}
