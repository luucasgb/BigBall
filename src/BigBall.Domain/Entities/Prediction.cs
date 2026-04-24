namespace BigBall.Domain.Entities;

public sealed class Prediction
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required Guid PoolId { get; init; }
    public required Guid MatchId { get; init; }
    public required int Home { get; set; }
    public required int Away { get; set; }
    public string? PenaltyWinnerCode { get; set; }
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}
