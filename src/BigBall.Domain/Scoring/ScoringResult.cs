namespace BigBall.Domain.Scoring;

public readonly record struct ScoringResult(int Tier, int Bonus)
{
    public int Total => Tier + Bonus;
}
