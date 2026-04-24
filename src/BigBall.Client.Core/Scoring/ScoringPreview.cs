using BigBall.Domain.Scoring;

namespace BigBall.Client.Core.Scoring;

public sealed record ScoringPreview(int Home, int Away, int Tier, int Bonus)
{
    public int Total => Tier + Bonus;

    public static ScoringPreview? TryCompute(
        int predHome,
        int predAway,
        int? referenceHome,
        int? referenceAway,
        string? predPenaltyWinner = null,
        string? actualPenaltyWinner = null)
    {
        if (referenceHome is null || referenceAway is null) return null;
        var result = ScoringEngine.Score(
            predHome, predAway,
            referenceHome.Value, referenceAway.Value,
            predPenaltyWinner, actualPenaltyWinner);
        return new ScoringPreview(predHome, predAway, result.Tier, result.Bonus);
    }
}
