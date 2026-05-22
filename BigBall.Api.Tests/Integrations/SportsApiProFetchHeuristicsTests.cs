using BigBall.Api.Integrations.SportsApiPro;

namespace BigBall.Api.Tests.Integrations;

public sealed class SportsApiProFetchHeuristicsTests
{
    [Fact]
    public void ShouldRequestPenalties_only_for_shootout_codes()
    {
        Assert.True(SportsApiProMapper.ShouldRequestPenaltiesEndpoint(50));
        Assert.True(SportsApiProMapper.ShouldRequestPenaltiesEndpoint(120));
        Assert.False(SportsApiProMapper.ShouldRequestPenaltiesEndpoint(100));
        Assert.False(SportsApiProMapper.ShouldRequestPenaltiesEndpoint(null));
    }

    [Fact]
    public void ShouldRequestScores_false_for_not_started()
    {
        const string minimal =
            "{\"event\":{\"id\":1,\"status\":{\"statusCode\":0},\"startTimestamp\":1,\"homeTeam\":{\"id\":2},\"awayTeam\":{\"id\":3}}}";

        Assert.False(SportsApiProMapper.ShouldRequestScoresEndpoint(minimal, 0));
    }
}
