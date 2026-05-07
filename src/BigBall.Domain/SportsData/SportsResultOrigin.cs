namespace BigBall.Domain.SportsData;

/// <summary>
/// Whether we can trust mapped fields for scoring (PRD / Tech Spec — manual vs feed, gaps).
/// </summary>
public enum SportsResultOrigin
{
    Unknown = 0,
    /// <summary>Full mapping from provider payload.</summary>
    ProviderComplete,
    /// <summary>Some fields filled; TR may still need manual resolution.</summary>
    ProviderPartial,
    /// <summary>Regular-time goals not reliably available from feed (gap).</summary>
    GapRegularTimeUnresolved
}
