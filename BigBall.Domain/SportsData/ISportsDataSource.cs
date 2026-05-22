namespace BigBall.Domain.SportsData;

/// <summary>
/// Port for external sports match data. Implementations are provider-specific (REST, etc.).
/// </summary>
public interface ISportsDataSource
{
    /// <summary>
    /// Fetch current view of a match by the provider's external id (string for cross-provider support).
    /// </summary>
    /// <inheritdoc cref="SportsMatchFetchTelemetry"/>
    Task<SportsMatchSnapshot?> GetMatchByExternalIdAsync(
        string externalMatchId,
        SportsMatchFetchTelemetry? telemetry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Scheduled matches for a calendar day (UTC date used with provider schedule semantics).
    /// </summary>
    Task<IReadOnlyList<SportsScheduledFixture>> GetScheduleAsync(
        DateOnly date,
        SportsMatchFetchTelemetry? telemetry,
        CancellationToken cancellationToken = default);
}
