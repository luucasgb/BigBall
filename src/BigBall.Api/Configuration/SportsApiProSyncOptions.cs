namespace BigBall.Api.Configuration;

/// <summary>Hosted match cadence (<see cref="MatchFeedSyncHostedService"/>) — JSON under <see cref="SportsApiProSyncOptions.SectionName"/>.</summary>
public sealed class SportsApiProSyncOptions
{
    public const string SectionName = "SportsApiPro:Sync";

    /// <summary>When false (default dev), HTTP sync worker does nothing.</summary>
    public bool Enabled { get; set; }

    /// <summary>Max aggregated HTTP GETs to SportsAPI Pro per UTC calendar day; null disables the check.</summary>
    public int? DailyRequestBudget { get; set; } = 100;

    /// <summary>Loop cadence — how often we re-evaluate which matches deserve a vendor refresh.</summary>
    public int TickSeconds { get; set; } = 60;

    /// <summary>Only poll matches whose kick-off is within this many minutes ahead (warm-up).</summary>
    public int WarmWindowBeforeKickoffMinutes { get; set; } = 60;

    /// <summary>Keep polling reasonably long after KO for live / unknown status (hours).</summary>
    public int PollHorizonHoursAfterKickoff { get; set; } = 10;

    /// <summary>Minimum seconds between polls while vendor reports not started / pre-match (status 0); see <see cref="BigBall.Api.Sync.MatchPollingIntervals"/>.</summary>
    public int SecondsPreMatchStale { get; set; } = 60 * 60 * 2;

    /// <summary>Minimum seconds between polls during first half (status 6).</summary>
    public int SecondsFirstHalf { get; set; } = 60;

    /// <summary>Minimum seconds between polls during second half (status 7).</summary>
    public int SecondsSecondHalf { get; set; } = 60;

    /// <summary>Minimum seconds between polls at halftime (status 31); also bounds unknown/null/80 polling via <see cref="BigBall.Api.Sync.MatchPollingIntervals"/>.</summary>
    public int SecondsHalftimeBreak { get; set; } = 60 * 15;

    /// <summary>Minimum seconds between polls during extra time or penalties (statuses 40, 41, 50).</summary>
    public int SecondsExtraOrPenalties { get; set; } = 60;

    /// <summary>Minimum seconds between polls after a terminal result (statuses 60, 70, 90, 100, 110, 120) until horizon cutoff.</summary>
    public int SecondsTerminalReconciliation { get; set; } = 480;
}
