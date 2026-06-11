namespace BigBall.Api.Configuration;

/// <summary>
/// Generic match-feed polling cadence + daily HTTP budget for
/// <see cref="BigBall.Api.Sync.MatchFeedSyncHostedService"/>.
/// Bound to JSON section <see cref="SectionName"/>; provider-agnostic.
/// </summary>
public sealed class MatchProviderSyncOptions
{
    public const string SectionName = "MatchProviderSync";

    /// <summary>When false (default dev), the HTTP sync worker does nothing.</summary>
    public bool Enabled { get; set; }

    /// <summary>Max aggregated outbound HTTP GETs to the provider per UTC calendar day; null disables the check.</summary>
    public int? DailyRequestBudget { get; set; } = 100;

    /// <summary>Loop cadence — how often we re-evaluate which matches deserve a vendor refresh.</summary>
    public int TickSeconds { get; set; } = 60;

    /// <summary>Only poll matches whose kick-off is within this many minutes ahead (warm-up).</summary>
    public int WarmWindowBeforeKickoffMinutes { get; set; } = 60;

    /// <summary>Keep polling reasonably long after KO for live / unknown phase (hours).</summary>
    public int PollHorizonHoursAfterKickoff { get; set; } = 10;

    /// <summary>Minimum seconds between polls while the canonical phase is <c>NotStarted</c>.</summary>
    public int SecondsPreMatchStale { get; set; } = 60 * 60 * 2;

    /// <summary>Minimum seconds between polls while the canonical phase is <c>FirstHalf</c>.</summary>
    public int SecondsFirstHalf { get; set; } = 60;

    /// <summary>Minimum seconds between polls while the canonical phase is <c>SecondHalf</c>.</summary>
    public int SecondsSecondHalf { get; set; } = 60;

    /// <summary>Minimum seconds between polls while the canonical phase is <c>Halftime</c>; also bounds <c>Unknown</c>/<c>Interrupted</c>.</summary>
    public int SecondsHalftimeBreak { get; set; } = 60 * 15;

    /// <summary>Minimum seconds between polls during extra time or penalties (<c>ExtraTime*</c>, <c>PenaltyShootoutInProgress</c>).</summary>
    public int SecondsExtraOrPenalties { get; set; } = 60;

    /// <summary>Minimum seconds between polls after a terminal phase (<c>Finished*</c>, <c>Postponed</c>, <c>Canceled</c>, <c>Abandoned</c>) until horizon cutoff.</summary>
    public int SecondsTerminalReconciliation { get; set; } = 480;
}
