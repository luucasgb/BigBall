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
    SportsResultOrigin ResultOrigin);
