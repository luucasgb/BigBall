using BigBall.Api.Configuration;
using BigBall.Api.Sync;

namespace BigBall.Api.Tests.Sync;

public sealed class MatchPollingIntervalsTests
{
    private readonly SportsApiProSyncOptions _o = new()
    {
        WarmWindowBeforeKickoffMinutes = 120,
        PollHorizonHoursAfterKickoff = 8,
        SecondsFirstHalf = 60,
        SecondsTerminalReconciliation = 300,
    };

    [Fact]
    public void IsDue_returns_false_far_before_kick_even_if_never_synced()
    {
        var ko = DateTime.UtcNow.AddDays(700);
        var utc = DateTime.UtcNow;

        Assert.False(MatchPollingIntervals.IsDue(utc, ko, 0, null, _o));
    }

    [Fact]
    public void IsDue_returns_true_inside_warm_when_never_synced()
    {
        var ko = DateTime.UtcNow.AddHours(18);
        var utcWithinWarm = ko - TimeSpan.FromMinutes(100);

        Assert.True(
            MatchPollingIntervals.IsDue(utcWithinWarm, ko, lastCode: null, lastSyncedUtcUtc: null, _o));
    }

    [Fact]
    public void LooksTerminalEnded_matches_documented_codes()
    {
        Assert.True(MatchPollingIntervals.LooksTerminalEnded(100));
        Assert.False(MatchPollingIntervals.LooksTerminalEnded(7));
    }
}
