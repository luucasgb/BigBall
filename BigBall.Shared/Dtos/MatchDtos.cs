namespace BigBall.Shared.Dtos;

public sealed record HostCityDto(
    int Id,
    string CityName,
    string Country,
    string VenueName,
    string RegionCluster,
    string AirportCode);

/// <summary>Range query result for calendar / fixture lists (no pool context).</summary>
public sealed record MatchCalendarRowDto(
    Guid Id,
    string Phase,
    string? GroupLabel,
    string HomeCode,
    string AwayCode,
    DateTime KickoffUtc,
    string? Venue,
    HostCityDto? HostCity,
    string Status);

public sealed record MatchSummaryDto(
    Guid Id,
    string Phase,
    string? GroupLabel,
    string HomeCode,
    string AwayCode,
    DateTime KickoffUtc,
    string Status,
    ScoreDto? MyPrediction);

public sealed record MatchDetailDto(
    Guid Id,
    string Phase,
    string? GroupLabel,
    string HomeCode,
    string AwayCode,
    DateTime KickoffUtc,
    DateTime LockUtc,
    string? Venue,
    HostCityDto? HostCity,
    string Status,
    int? ReferenceHome,
    int? ReferenceAway,
    bool WentToPenalties,
    string? PenaltyWinnerCode,
    PredictionDto? MyPrediction);

public sealed record ScoreDto(int Home, int Away);

/// <summary>A user's prediction for one match within a single pool (null when not yet predicted).</summary>
public sealed record PoolPredictionDto(Guid PoolId, ScoreDto? Prediction);

/// <summary>One match as seen inside a pool: the member's own prediction alongside the real
/// (reference) score. <see cref="ReferenceHome"/>/<see cref="ReferenceAway"/> are null until the
/// match produces a result, in which case the UI shows the kickoff time instead.</summary>
public sealed record PoolMatchRowDto(
    Guid Id,
    string Phase,
    string? GroupLabel,
    string HomeCode,
    string AwayCode,
    DateTime KickoffUtc,
    string Status,
    int? ReferenceHome,
    int? ReferenceAway,
    ScoreDto? MyPrediction);
