using BigBall.Api.Configuration;
using BigBall.Api.Sync;
using BigBall.Domain.SportsData;

namespace BigBall.Api.Tests.Sync;

public sealed class MatchPollingIntervalsTests
{
    private readonly MatchProviderSyncOptions _o = new()
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

        Assert.False(MatchPollingIntervals.IsDue(utc, ko, MatchLifecyclePhase.NotStarted, null, _o));
    }

    [Fact]
    public void IsDue_returns_true_inside_warm_when_never_synced()
    {
        var ko = DateTime.UtcNow.AddHours(18);
        var utcWithinWarm = ko - TimeSpan.FromMinutes(100);

        Assert.True(
            MatchPollingIntervals.IsDue(utcWithinWarm, ko, MatchLifecyclePhase.Unknown, lastSyncedUtcUtc: null, _o));
    }

    [Theory]
    [InlineData(MatchLifecyclePhase.FinishedRegulation, true)]
    [InlineData(MatchLifecyclePhase.FinishedAfterExtraTime, true)]
    [InlineData(MatchLifecyclePhase.FinishedAfterPenalties, true)]
    [InlineData(MatchLifecyclePhase.Postponed, true)]
    [InlineData(MatchLifecyclePhase.Canceled, true)]
    [InlineData(MatchLifecyclePhase.Abandoned, true)]
    [InlineData(MatchLifecyclePhase.SecondHalf, false)]
    [InlineData(MatchLifecyclePhase.NotStarted, false)]
    public void LooksTerminalEnded_matches_phase_table(MatchLifecyclePhase phase, bool expected)
        => Assert.Equal(expected, MatchPollingIntervals.LooksTerminalEnded(phase));

    [Fact]
    public void GapForPhase_uses_first_half_seconds()
    {
        var gap = MatchPollingIntervals.GapForPhase(_o, MatchLifecyclePhase.FirstHalf);
        Assert.Equal(TimeSpan.FromSeconds(60), gap);
    }
}
