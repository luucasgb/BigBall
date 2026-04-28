using BigBall.Api.Configuration;
using BigBall.Api.Integrations.SportsApiPro;
using BigBall.Domain.SportsData;

namespace BigBall.Api.Tests;

public sealed class SportsApiProMapperTests
{
    [Theory]
    [InlineData(null, MatchLifecyclePhase.Unknown)]
    [InlineData(100, MatchLifecyclePhase.FinishedRegulation)]
    [InlineData(110, MatchLifecyclePhase.FinishedAfterExtraTime)]
    [InlineData(120, MatchLifecyclePhase.FinishedAfterPenalties)]
    public void MapLifecycleFromStatus_known_codes_match_domain(int? code, MatchLifecyclePhase expected)
        => Assert.Equal(expected, SportsApiProMapper.MapLifecycleFromStatus(code));

    [Fact]
    public void Map_finished_regulation_uses_half_sum_when_normaltime_absent()
    {
        const string matchJson = """
        {
          "event": {
            "id": 14025056,
            "startTimestamp": 1710000000,
            "status": { "code": 100 },
            "homeScore": { "period1": 1, "period2": 1 },
            "awayScore": { "period1": 0, "period2": 0 }
          }
        }
        """;

        var s = SportsApiProMapper.Map(matchJson);

        Assert.Equal("14025056", s.ExternalMatchId);
        Assert.Equal(MatchLifecyclePhase.FinishedRegulation, s.Phase);
        Assert.Equal(2, s.GoalsHomeRegularTime);
        Assert.Equal(0, s.GoalsAwayRegularTime);
        Assert.True(s.RegularTimeScoresReliable);
        Assert.Equal(SportsResultOrigin.ProviderComplete, s.ResultOrigin);
        Assert.Equal(SportsDataProviderNames.SportsApiPro, s.ProviderName);
    }

    [Fact]
    public void Map_prefers_normaltime_when_present()
    {
        const string matchJson = """
        {
          "event": {
            "id": 1,
            "startTimestamp": 1710000000,
            "status": { "code": 120 },
            "homeScore": { "normaltime": 1, "period1": 0, "period2": 1 },
            "awayScore": { "normaltime": 1, "period1": 1, "period2": 0 }
          }
        }
        """;

        var s = SportsApiProMapper.Map(matchJson);

        Assert.Equal(1, s.GoalsHomeRegularTime);
        Assert.Equal(1, s.GoalsAwayRegularTime);
        Assert.True(s.WentToPenaltyShootout);
    }
}
