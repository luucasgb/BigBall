namespace BigBall.Domain.SportsData;

/// <summary>
/// Coarse lifecycle of a match from an external feed (mapped from provider-specific status codes).
/// </summary>
public enum MatchLifecyclePhase
{
    Unknown = 0,
    NotStarted,
    FirstHalf,
    SecondHalf,
    Halftime,
    ExtraTimeFirstHalf,
    ExtraTimeSecondHalf,
    PenaltyShootoutInProgress,
    /// <summary>Ended in regulation (90 min + stoppage).</summary>
    FinishedRegulation,
    FinishedAfterExtraTime,
    FinishedAfterPenalties,
    Postponed,
    Canceled,
    Interrupted,
    Abandoned
}
