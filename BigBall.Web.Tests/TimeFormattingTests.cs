using BigBall.Web.Shared.UI;

namespace BigBall.Web.Tests;

public class TimeFormattingTests
{
    [Fact]
    public void FormatPredictionDeadlineCountdown_ZeroOrNegative_Yields_0m()
    {
        Assert.Equal("0m", TimeFormatting.FormatPredictionDeadlineCountdown(TimeSpan.Zero));
        Assert.Equal("0m", TimeFormatting.FormatPredictionDeadlineCountdown(TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void FormatPredictionDeadlineCountdown_Over7Days_DaysOnly()
    {
        Assert.Equal("10d", TimeFormatting.FormatPredictionDeadlineCountdown(TimeSpan.FromDays(10)));
        Assert.Equal("8d", TimeFormatting.FormatPredictionDeadlineCountdown(TimeSpan.FromDays(8) + TimeSpan.FromHours(12)));
    }

    [Fact]
    public void FormatPredictionDeadlineCountdown_Exactly7Days_DaysAndHours_Tier()
    {
        Assert.Equal("7d 0h", TimeFormatting.FormatPredictionDeadlineCountdown(TimeSpan.FromDays(7)));
    }

    [Fact]
    public void FormatPredictionDeadlineCountdown_Between12hAnd7d_DaysAndHours()
    {
        Assert.Equal("6d 14h", TimeFormatting.FormatPredictionDeadlineCountdown(TimeSpan.FromDays(6) + TimeSpan.FromHours(14)));
    }

    [Fact]
    public void FormatPredictionDeadlineCountdown_Over12h_NoFullDays_Uses_0dXh()
    {
        Assert.Equal("0d 15h", TimeFormatting.FormatPredictionDeadlineCountdown(TimeSpan.FromHours(15)));
    }

    [Fact]
    public void FormatPredictionDeadlineCountdown_Exactly12h_HoursAndMinutes()
    {
        Assert.Equal("12h 0m", TimeFormatting.FormatPredictionDeadlineCountdown(TimeSpan.FromHours(12)));
    }

    [Fact]
    public void FormatPredictionDeadlineCountdown_Between5mAnd12h_HoursAndMinutes()
    {
        Assert.Equal("11h 30m", TimeFormatting.FormatPredictionDeadlineCountdown(TimeSpan.FromHours(11) + TimeSpan.FromMinutes(30)));
        Assert.Equal("0h 6m", TimeFormatting.FormatPredictionDeadlineCountdown(TimeSpan.FromMinutes(6)));
    }

    [Fact]
    public void FormatPredictionDeadlineCountdown_At5m_Uses_MinutesAndSeconds_Tier()
    {
        Assert.Equal("5m 00s", TimeFormatting.FormatPredictionDeadlineCountdown(TimeSpan.FromMinutes(5)));
    }

    [Fact]
    public void FormatPredictionDeadlineCountdown_Under5m_MinutesAndSeconds()
    {
        Assert.Equal("3m 30s", TimeFormatting.FormatPredictionDeadlineCountdown(TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(30)));
        Assert.Equal("0m 45s", TimeFormatting.FormatPredictionDeadlineCountdown(TimeSpan.FromSeconds(45)));
    }
}
