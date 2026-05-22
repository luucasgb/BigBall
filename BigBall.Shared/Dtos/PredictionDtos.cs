namespace BigBall.Shared.Dtos;

public sealed record PredictionDto(
    Guid MatchId,
    int Home,
    int Away,
    string? PenaltyWinnerCode,
    DateTime UpdatedUtc);

public sealed record UpsertPredictionRequest(int Home, int Away, string? PenaltyWinnerCode);

public sealed record LockedError(string Code, string Message)
{
    public static LockedError Default { get; } = new("LOCKED", "Palpite bloqueado: a partida já começou.");
}
