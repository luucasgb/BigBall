namespace BigBall.Domain.SportsData;

/// <summary>
/// Provider-agnostic snapshot for sync into <see cref="BigBall.Domain.Entities.Match"/> (Tech Spec §6.2).
/// </summary>
public sealed record SportsMatchSnapshot(
    string ProviderName,
    string ExternalMatchId,
    DateTime KickoffUtc,
    MatchLifecyclePhase Phase,
    /// <summary>Raw provider match status code when available (SportsAPI Pro <c>status.code</c>).</summary>
    int? ProviderStatusCode,
    int? GoalsHomeRegularTime,
    int? GoalsAwayRegularTime,
    bool RegularTimeScoresReliable,
    bool WentToExtraTime,
    bool WentToPenaltyShootout,
    /// <summary>When <see cref="WentToPenaltyShootout"/> is true: home won if true, away if false.</summary>
    bool? PenaltyWinnerIsHome,
    SportsResultOrigin ResultOrigin)
{
    /// <summary>Home team badge URL (FlashScore <c>home_team.image_path</c>); null when not exposed by the provider.</summary>
    public string? HomeTeamImageUrl { get; init; }
    public string? HomeTeamImageUrlSmall { get; init; }
    public string? HomeTeamFlashScoreId { get; init; }
    public string? HomeTeamFlashScoreUrl { get; init; }

    /// <summary>Away team badge URL (FlashScore <c>away_team.image_path</c>); null when not exposed by the provider.</summary>
    public string? AwayTeamImageUrl { get; init; }
    public string? AwayTeamImageUrlSmall { get; init; }
    public string? AwayTeamFlashScoreId { get; init; }
    public string? AwayTeamFlashScoreUrl { get; init; }
}
