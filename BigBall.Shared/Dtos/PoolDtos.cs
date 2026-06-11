namespace BigBall.Shared.Dtos;

public sealed record MyPoolDto(
    Guid Id,
    string Name,
    int MemberCount,
    int MyRank,
    int MyPoints,
    LeaderDto Leader,
    NextMatchDto? NextMatch);

public sealed record LeaderDto(Guid UserId, string Name, int Points);

public sealed record NextMatchDto(
    Guid Id,
    string HomeCode,
    string AwayCode,
    DateTime KickoffUtc,
    string? GroupLabel,
    ScoreDto? MyPrediction);

public sealed record PoolDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string Visibility,
    string? InviteCode,
    string? PrizeDescription,
    string? EntryCost,
    int MyPosition,
    int MyPoints,
    LeaderDto Leader,
    IReadOnlyList<RankingRowDto> Ranking);

public sealed record RankingRowDto(
    int Rank,
    Guid UserId,
    string Name,
    int Points,
    int Tier20Count,
    int Tier16Count,
    int Tier15Count,
    int Tier10Count,
    int Tier5Count,
    int PenaltyBonusCount,
    int TrendLastMatch,
    int PredictedGamesCount,
    bool IsMe,
    int? TieGroupId);

public sealed record CreatePoolRequest(
    string Name,
    string? Description,
    string Visibility,
    string PrizeDescription,
    string? EntryCost);

public sealed record CreatePoolResponse(Guid PoolId, string? InviteCode);

public sealed record JoinPoolRequest(string InviteCode);

public sealed record JoinPoolResponse(Guid PoolId, string PoolName);
