using BigBall.Api.Data;
using BigBall.Api.Sync;
using BigBall.Domain.Entities;
using BigBall.Domain.Enums;
using BigBall.Domain.SportsData;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Tests.Sync;

public sealed class SportsMatchFeedSyncApplierTests
{
    private static BigBallDbContext NewDb()
    {
        var opts = new DbContextOptionsBuilder<BigBallDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BigBallDbContext(opts);
    }

    private static Match NewMatch(string home = "BRA", string away = "ARG") => new()
    {
        Id = Guid.NewGuid(),
        HomeCode = home,
        AwayCode = away,
        KickoffUtc = new DateTime(2026, 6, 15, 18, 0, 0, DateTimeKind.Utc),
        Phase = MatchPhase.Groups,
    };

    private static SportsMatchSnapshot SnapshotWithTeams(
        string? homeUrl = "https://x/home.png",
        string? awayUrl = "https://x/away.png") =>
        new(
            ProviderName: "FlashScore",
            ExternalMatchId: "abc",
            KickoffUtc: DateTime.UtcNow,
            Phase: MatchLifecyclePhase.FinishedRegulation,
            ProviderStatusCode: null,
            GoalsHomeRegularTime: 2,
            GoalsAwayRegularTime: 1,
            RegularTimeScoresReliable: true,
            WentToExtraTime: false,
            WentToPenaltyShootout: false,
            PenaltyWinnerIsHome: null,
            ResultOrigin: SportsResultOrigin.ProviderComplete)
        {
            HomeTeamImageUrl = homeUrl,
            HomeTeamImageUrlSmall = homeUrl is null ? null : "https://x/home_s.png",
            HomeTeamFlashScoreId = "h1",
            HomeTeamFlashScoreUrl = "/team/brazil/h1/",
            AwayTeamImageUrl = awayUrl,
            AwayTeamImageUrlSmall = awayUrl is null ? null : "https://x/away_s.png",
            AwayTeamFlashScoreId = "a1",
            AwayTeamFlashScoreUrl = "/team/argentina/a1/",
        };

    [Fact]
    public async Task ApplyAsync_inserts_team_rows_with_urls_on_first_sync()
    {
        await using var db = NewDb();
        var match = NewMatch();
        db.Matches.Add(match);
        await db.SaveChangesAsync();

        var now = DateTime.UtcNow;
        await SportsMatchFeedSyncApplier.ApplyAsync(db, match, SnapshotWithTeams(), now, now, default);
        await db.SaveChangesAsync();

        var bra = await db.Teams.SingleAsync(t => t.Code == "BRA");
        var arg = await db.Teams.SingleAsync(t => t.Code == "ARG");

        Assert.Equal("https://x/home.png", bra.BadgeUrl);
        Assert.Equal("h1", bra.FlashScoreTeamId);
        Assert.Equal("match-sync", bra.LastSource);
        Assert.Equal(now, bra.LastUpdatedUtc);

        Assert.Equal("https://x/away.png", arg.BadgeUrl);
        Assert.Equal("/team/argentina/a1/", arg.FlashScoreTeamUrl);
    }

    [Fact]
    public async Task ApplyAsync_does_not_overwrite_existing_url_with_null()
    {
        await using var db = NewDb();
        db.Teams.Add(new Team
        {
            Code = "BRA",
            DisplayName = "Brazil",
            BadgeUrl = "https://x/cached.png",
            BadgeUrlSmall = "https://x/cached_s.png",
            FlashScoreTeamId = "cached-id",
            LastSource = "search",
            LastUpdatedUtc = DateTime.UtcNow.AddDays(-1),
        });
        var match = NewMatch();
        db.Matches.Add(match);
        await db.SaveChangesAsync();

        // Snapshot with all home-side fields null (provider omitted the home_team block entirely)
        // must leave the cached row untouched on those fields.
        var snap = new SportsMatchSnapshot(
            ProviderName: "FlashScore",
            ExternalMatchId: "abc",
            KickoffUtc: DateTime.UtcNow,
            Phase: MatchLifecyclePhase.FinishedRegulation,
            ProviderStatusCode: null,
            GoalsHomeRegularTime: 2,
            GoalsAwayRegularTime: 1,
            RegularTimeScoresReliable: true,
            WentToExtraTime: false,
            WentToPenaltyShootout: false,
            PenaltyWinnerIsHome: null,
            ResultOrigin: SportsResultOrigin.ProviderComplete);

        var now = DateTime.UtcNow;
        await SportsMatchFeedSyncApplier.ApplyAsync(db, match, snap, now, now, default);
        await db.SaveChangesAsync();

        var bra = await db.Teams.SingleAsync(t => t.Code == "BRA");
        Assert.Equal("https://x/cached.png", bra.BadgeUrl);
        Assert.Equal("https://x/cached_s.png", bra.BadgeUrlSmall);
        Assert.Equal("cached-id", bra.FlashScoreTeamId);
        Assert.Equal("search", bra.LastSource);
    }

    [Fact]
    public async Task ApplyAsync_preserves_url_when_snapshot_has_id_but_null_url()
    {
        await using var db = NewDb();
        db.Teams.Add(new Team
        {
            Code = "BRA",
            DisplayName = "Brazil",
            BadgeUrl = "https://x/cached.png",
            LastSource = "search",
            LastUpdatedUtc = DateTime.UtcNow.AddDays(-1),
        });
        var match = NewMatch();
        db.Matches.Add(match);
        await db.SaveChangesAsync();

        // Per-field null-no-overwrite: the URL stays "cached.png" even though the snapshot has a fresh id.
        var now = DateTime.UtcNow;
        await SportsMatchFeedSyncApplier.ApplyAsync(db, match, SnapshotWithTeams(homeUrl: null), now, now, default);
        await db.SaveChangesAsync();

        var bra = await db.Teams.SingleAsync(t => t.Code == "BRA");
        Assert.Equal("https://x/cached.png", bra.BadgeUrl);
        // Other fields (id, source, timestamp) may update — that's intentional refresh.
        Assert.Equal("h1", bra.FlashScoreTeamId);
        Assert.Equal("match-sync", bra.LastSource);
    }

    [Fact]
    public async Task ApplyAsync_updates_existing_team_row_when_snapshot_has_new_url()
    {
        await using var db = NewDb();
        db.Teams.Add(new Team
        {
            Code = "BRA",
            DisplayName = "Brazil",
            BadgeUrl = "https://x/old.png",
            LastSource = "search",
            LastUpdatedUtc = DateTime.UtcNow.AddDays(-1),
        });
        var match = NewMatch();
        db.Matches.Add(match);
        await db.SaveChangesAsync();

        var now = DateTime.UtcNow;
        await SportsMatchFeedSyncApplier.ApplyAsync(db, match, SnapshotWithTeams(homeUrl: "https://x/new.png"), now, now, default);
        await db.SaveChangesAsync();

        var bra = await db.Teams.SingleAsync(t => t.Code == "BRA");
        Assert.Equal("https://x/new.png", bra.BadgeUrl);
        Assert.Equal("match-sync", bra.LastSource);
        Assert.Equal(now, bra.LastUpdatedUtc);
    }
}
