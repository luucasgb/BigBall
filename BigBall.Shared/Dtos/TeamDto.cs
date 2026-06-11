namespace BigBall.Shared.Dtos;

/// <summary>Lightweight team-badge lookup row served by <c>GET /api/teams</c> and cached client-side.</summary>
public sealed record TeamDto(
    string Code,
    string DisplayName,
    string? BadgeUrl,
    string? BadgeUrlSmall,
    string? CountryImageUrl);

/// <summary>Response payload of the one-shot WC2026 badge backfill admin endpoint.</summary>
public sealed record TeamBackfillReportDto(
    int TotalRequested,
    int TotalUpdated,
    int TotalSkippedAlreadyCached,
    int HttpCallsUsed,
    IReadOnlyList<string> UnresolvedCodes,
    IReadOnlyList<string> Errors);
