namespace BigBall.Domain.Entities;

/// <summary>
/// Cached lookup row for a team code (e.g. "BRA"). Populated either by a one-shot
/// FlashScore Search backfill or passively by the match-sync pipeline reading
/// <c>home_team</c>/<c>away_team</c> image fields from each match payload.
/// Decoupled from <see cref="Match"/> — the join key is the 3-letter code string.
/// </summary>
public sealed class Team
{
    /// <summary>3-letter FIFA-style code (matches <see cref="Match.HomeCode"/>).</summary>
    public required string Code { get; init; }

    /// <summary>Country / team display name (e.g. "Brazil"). Seeded from WorldCup2026TeamCodes.</summary>
    public required string DisplayName { get; set; }

    /// <summary>Full-resolution badge URL from FlashScore (<c>image_path</c>); null until discovered.</summary>
    public string? BadgeUrl { get; set; }

    /// <summary>Small badge URL from FlashScore (<c>small_image_path</c>); null until discovered.</summary>
    public string? BadgeUrlSmall { get; set; }

    /// <summary>Confederation flag URL from FlashScore (<c>country_image_path</c>).</summary>
    public string? CountryImageUrl { get; set; }

    /// <summary>FlashScore team id (e.g. "I9l9aqLq").</summary>
    public string? FlashScoreTeamId { get; set; }

    /// <summary>FlashScore team url slug (e.g. "brazil").</summary>
    public string? FlashScoreTeamUrl { get; set; }

    public DateTime? LastUpdatedUtc { get; set; }

    /// <summary>"search" (one-shot backfill) or "match-sync" (passive refresh).</summary>
    public string? LastSource { get; set; }
}
