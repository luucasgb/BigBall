using System.Text.Json.Serialization;

namespace BigBall.Shared.Dtos;

/// <summary>Result of the dev-only SportsApi Pro Football V2 World Cup 2026 schedule probe.</summary>
public sealed record WorldCup2026SportsApiProbeDto(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("error")] string? Error,
    [property: JsonPropertyName("totalUniqueMatches")] int TotalUniqueMatches,
    [property: JsonPropertyName("roundsQueried")] int RoundsQueried,
    [property: JsonPropertyName("seasonLabel")] string? SeasonLabel,
    [property: JsonPropertyName("vendorSeasonId")] long? VendorSeasonId,
    [property: JsonPropertyName("vendorTournamentId")] long? VendorTournamentId,
    [property: JsonPropertyName("aggregateJsonLength")] int AggregateJsonLength,
    [property: JsonPropertyName("aggregateJsonPreview")] string? AggregateJsonPreview);
