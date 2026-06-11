namespace BigBall.Domain.Entities;

/// <summary>Accumulated HTTP GET count to the configured sports data provider for quota tracking (UTC calendar day).</summary>
public sealed class ProviderDailyApiUsage
{
    /// <summary>UTC date (stored as date-only semantics).</summary>
    public DateOnly DayUtc { get; init; }

    public int HttpGetCount { get; set; }
}
