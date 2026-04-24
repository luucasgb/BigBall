namespace BigBall.Domain.Scoring;

/// <summary>
/// Pure scoring engine — PRD §4.8. Deterministic: same inputs → same result.
/// Tiers 20/16/15/10/5/0 on regulation-time score; +3 bonus on correct penalty winner.
/// </summary>
public static class ScoringEngine
{
    public static ScoringResult Score(
        int predHome, int predAway,
        int actualHome, int actualAway,
        string? predPenaltyWinnerCode = null,
        string? actualPenaltyWinnerCode = null)
    {
        int tier = ComputeTier(predHome, predAway, actualHome, actualAway);
        int bonus = 0;
        if (!string.IsNullOrWhiteSpace(actualPenaltyWinnerCode)
            && !string.IsNullOrWhiteSpace(predPenaltyWinnerCode)
            && string.Equals(predPenaltyWinnerCode, actualPenaltyWinnerCode, StringComparison.OrdinalIgnoreCase))
        {
            bonus = 3;
        }
        return new ScoringResult(tier, bonus);
    }

    private static int ComputeTier(int ph, int pa, int ah, int aa)
    {
        // 20 — exact score
        if (ph == ah && pa == aa) return 20;

        int predSign = Sign(ph - pa);
        int actualSign = Sign(ah - aa);
        int predDiff = ph - pa;
        int actualDiff = ah - aa;

        bool sameWinner = predSign == actualSign;
        bool sameDiff = predDiff == actualDiff;
        bool oneSideMatches = (ph == ah) || (pa == aa);
        bool bothDraw = predSign == 0 && actualSign == 0;

        // 16 — (winner + diff) OR draw-with-different-score
        if ((sameWinner && !bothDraw && sameDiff) || (bothDraw && !(ph == ah && pa == aa)))
            return 16;

        // 15 — winner + one side's score
        if (sameWinner && !bothDraw && oneSideMatches)
            return 15;

        // 10 — winner only
        if (sameWinner && !bothDraw)
            return 10;

        // 5 — one side's score only (wrong winner)
        if (oneSideMatches)
            return 5;

        return 0;
    }

    private static int Sign(int value) => value > 0 ? 1 : value < 0 ? -1 : 0;
}
