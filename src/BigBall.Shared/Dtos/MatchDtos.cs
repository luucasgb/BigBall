namespace BigBall.Shared.Dtos;

/// <summary>Range query result for calendar / fixture lists (no pool context).</summary>
public sealed record MatchCalendarRowDto(
    Guid Id,
    string Phase,
    string? GroupLabel,
    string HomeCode,
    string AwayCode,
    DateTime KickoffUtc,
    string? Venue,
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
    string Status,
    int? ReferenceHome,
    int? ReferenceAway,
    bool WentToPenalties,
    string? PenaltyWinnerCode,
    PredictionDto? MyPrediction);

public sealed record ScoreDto(int Home, int Away);
