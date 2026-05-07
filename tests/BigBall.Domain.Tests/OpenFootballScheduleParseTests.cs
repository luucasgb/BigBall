using BigBall.Shared.WorldCup;

namespace BigBall.Domain.Tests;

public class OpenFootballScheduleParseTests
{
    [Theory]
    [InlineData("2026-06-11", "13:00 UTC-6", 19, 0)] // 13:00 at -6 => 19:00 Z
    [InlineData("2026-06-27", "19:30 UTC-4", 23, 30)]
    [InlineData("2026-06-14", "15:00 UTC-5", 20, 0)]
    public void TryParseKickoffUtc_ConvertsUsingOffset(string date, string time, int expectedHourUtc, int expectedMinUtc)
    {
        Assert.True(OpenFootballScheduleParse.TryParseKickoffUtc(date, time, out var utc));
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
        Assert.Equal(expectedHourUtc, utc.Hour);
        Assert.Equal(expectedMinUtc, utc.Minute);
    }

    [Theory]
    [InlineData("Matchday 1", "Groups")]
    [InlineData("Round of 32", "RoundOf32")]
    [InlineData("Round of 16", "RoundOf16")]
    [InlineData("Quarter-final", "Quarters")]
    [InlineData("Semi-final", "Semis")]
    [InlineData("Match for third place", "ThirdPlace")]
    [InlineData("Final", "Final")]
    public void MapRoundToPhaseString_MapsLabels(string round, string expectedPhase)
    {
        Assert.Equal(expectedPhase, OpenFootballScheduleParse.MapRoundToPhaseString(round));
    }

    [Fact]
    public void BuildExternalKey_UsesNumWhenPresent()
    {
        var k = OpenFootballScheduleParse.BuildExternalKey(79, "2026-06-30", "19:00 UTC-6", "1A", "X", "Round of 32");
        Assert.Equal("wc2026-79", k);
    }
}
