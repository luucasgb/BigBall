using BigBall.Domain.Scoring;

namespace BigBall.Domain.Tests;

public class ScoringEngineTests
{
    [Theory]
    // 20 — exact
    [InlineData(2, 1, 2, 1, 20)]
    [InlineData(0, 0, 0, 0, 20)]
    // 16 — winner + diff
    [InlineData(3, 2, 2, 1, 16)]
    [InlineData(4, 1, 3, 0, 16)]
    // 16 — draw with different score
    [InlineData(1, 1, 2, 2, 16)]
    // 15 — winner + one side
    [InlineData(2, 0, 2, 1, 15)]
    [InlineData(3, 1, 2, 1, 15)]
    // 10 — winner only
    [InlineData(4, 2, 2, 1, 10)]
    // 5 — one side only, wrong winner
    [InlineData(1, 1, 2, 1, 5)]
    [InlineData(0, 2, 2, 2, 5)]
    // 0 — nothing right
    [InlineData(0, 2, 3, 1, 0)]
    public void Score_ComputesExpectedTier(int ph, int pa, int ah, int aa, int expectedTier)
    {
        var result = ScoringEngine.Score(ph, pa, ah, aa);
        Assert.Equal(expectedTier, result.Tier);
        Assert.Equal(0, result.Bonus);
        Assert.Equal(expectedTier, result.Total);
    }

    [Fact]
    public void Score_AddsPenaltyBonus_WhenPredictedWinnerMatches()
    {
        var r = ScoringEngine.Score(1, 1, 1, 1, predPenaltyWinnerCode: "BRA", actualPenaltyWinnerCode: "BRA");
        Assert.Equal(20, r.Tier);
        Assert.Equal(3, r.Bonus);
        Assert.Equal(23, r.Total);
    }

    [Fact]
    public void Score_NoPenaltyBonus_WhenPredictedWinnerDiffers()
    {
        var r = ScoringEngine.Score(1, 1, 1, 1, predPenaltyWinnerCode: "BRA", actualPenaltyWinnerCode: "ARG");
        Assert.Equal(0, r.Bonus);
    }

    [Fact]
    public void Score_NoPenaltyBonus_WhenNoPenaltyInMatch()
    {
        var r = ScoringEngine.Score(2, 1, 2, 1, predPenaltyWinnerCode: "BRA", actualPenaltyWinnerCode: null);
        Assert.Equal(0, r.Bonus);
    }
}
