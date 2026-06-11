using BigBall.Api.Configuration;
using BigBall.Api.Integrations.FlashScore;
using BigBall.Domain.SportsData;

namespace BigBall.Api.Tests.Integrations;

public sealed class FlashScoreMapperTests
{
    private const string FinishedRegulationJson = """
    {
      "match_id": "GCxZ2uHc",
      "match_status": {
        "stage": "Finished",
        "is_cancelled": false,
        "is_postponed": false,
        "is_started": true,
        "is_in_progress": false,
        "is_finished": true,
        "is_finished_after_extra_time": false,
        "is_finished_after_penalties": false,
        "winner": "away",
        "final_winner": "away"
      },
      "timestamp": 1747594466,
      "home_team": {
        "team_id": "h8oAv4Ts",
        "team_url": "/team/sevilla/h8oAv4Ts/",
        "name": "Sevilla",
        "short_name": "SEV",
        "image_path": "https://static.flashscore.com/res/image/data/G2BgO5Ar-tIT0KyhE.png",
        "small_image_path": "https://static.flashscore.com/res/image/data/EcTGi086-tIT0KyhE.png"
      },
      "away_team": {
        "team_id": "W8mj7MDD",
        "team_url": "/team/real-madrid/W8mj7MDD/",
        "name": "Real Madrid",
        "short_name": "RMA",
        "image_path": "https://static.flashscore.com/res/image/data/A7kHoxZA-ttfpEDUq.png",
        "small_image_path": "https://static.flashscore.com/res/image/data/CGPhnpne-ttfpEDUq.png"
      },
      "scores": {
        "home": 0, "away": 2,
        "home_total": 0, "away_total": 2,
        "home_1st_half": 0, "away_1st_half": 0,
        "home_2nd_half": 0, "away_2nd_half": 2,
        "home_extra_time": null, "away_extra_time": null,
        "home_penalties": null, "away_penalties": null
      }
    }
    """;

    [Fact]
    public void Map_finished_regulation_uses_totals_and_marks_provider_complete()
    {
        var snap = FlashScoreMapper.Map(FinishedRegulationJson);

        Assert.Equal(SportsDataProviderNames.FlashScore, snap.ProviderName);
        Assert.Equal("GCxZ2uHc", snap.ExternalMatchId);
        Assert.Equal(MatchLifecyclePhase.FinishedRegulation, snap.Phase);
        Assert.Equal(0, snap.GoalsHomeRegularTime);
        Assert.Equal(2, snap.GoalsAwayRegularTime);
        Assert.True(snap.RegularTimeScoresReliable);
        Assert.False(snap.WentToExtraTime);
        Assert.False(snap.WentToPenaltyShootout);
        Assert.Null(snap.PenaltyWinnerIsHome);
        Assert.Null(snap.ProviderStatusCode);
        Assert.Equal(SportsResultOrigin.ProviderComplete, snap.ResultOrigin);
    }

    [Fact]
    public void Map_finished_after_penalties_extracts_winner_from_final_winner()
    {
        const string json = """
        {
          "match_id": "ABC123",
          "match_status": {
            "is_started": true,
            "is_in_progress": false,
            "is_finished": true,
            "is_finished_after_extra_time": false,
            "is_finished_after_penalties": true,
            "winner": "home",
            "final_winner": "home"
          },
          "timestamp": 1700000000,
          "scores": {
            "home_total": 1, "away_total": 1,
            "home_1st_half": 0, "away_1st_half": 1,
            "home_2nd_half": 1, "away_2nd_half": 0,
            "home_extra_time": 0, "away_extra_time": 0,
            "home_penalties": 4, "away_penalties": 3
          }
        }
        """;

        var snap = FlashScoreMapper.Map(json);

        Assert.Equal(MatchLifecyclePhase.FinishedAfterPenalties, snap.Phase);
        Assert.True(snap.WentToPenaltyShootout);
        Assert.True(snap.WentToExtraTime);
        Assert.True(snap.PenaltyWinnerIsHome);
        Assert.Equal(1, snap.GoalsHomeRegularTime);
        Assert.Equal(1, snap.GoalsAwayRegularTime);
    }

    [Fact]
    public void Map_not_started_yields_unreliable_tr_and_unknown_origin()
    {
        const string json = """
        {
          "match_id": "NS1",
          "match_status": {
            "is_started": false,
            "is_in_progress": false,
            "is_finished": false,
            "is_cancelled": false,
            "is_postponed": false,
            "is_finished_after_extra_time": false,
            "is_finished_after_penalties": false
          },
          "timestamp": 1900000000,
          "scores": {
            "home_total": null, "away_total": null
          }
        }
        """;

        var snap = FlashScoreMapper.Map(json);

        Assert.Equal(MatchLifecyclePhase.NotStarted, snap.Phase);
        Assert.False(snap.RegularTimeScoresReliable);
        Assert.Null(snap.GoalsHomeRegularTime);
        Assert.Null(snap.GoalsAwayRegularTime);
        Assert.Equal(SportsResultOrigin.Unknown, snap.ResultOrigin);
    }

    [Theory]
    [InlineData("home", true)]
    [InlineData("away", false)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void ExtractPenaltyWinnerIsHome_reads_final_winner_when_shootout_finished(
        string? finalWinner, bool? expected)
    {
        var statusJson = finalWinner is null
            ? """{"is_finished_after_penalties": true}"""
            : $$"""{"is_finished_after_penalties": true, "final_winner": "{{finalWinner}}"}""";
        var node = System.Text.Json.Nodes.JsonNode.Parse(statusJson)!.AsObject();

        Assert.Equal(expected, FlashScoreMapper.ExtractPenaltyWinnerIsHome(node));
    }

    [Fact]
    public void ExtractPenaltyWinnerIsHome_returns_null_when_no_shootout()
    {
        var node = System.Text.Json.Nodes.JsonNode.Parse(
            """{"is_finished_after_penalties": false, "final_winner": "home"}""")!.AsObject();

        Assert.Null(FlashScoreMapper.ExtractPenaltyWinnerIsHome(node));
    }

    [Fact]
    public void ShouldRequestPenaltiesEndpoint_true_when_finished_after_penalties()
    {
        const string json = """{"match_status": {"is_finished_after_penalties": true}, "scores": {}}""";
        Assert.True(FlashScoreMapper.ShouldRequestPenaltiesEndpoint(json));
    }

    [Fact]
    public void ShouldRequestPenaltiesEndpoint_true_when_penalty_scores_present()
    {
        const string json = """{"match_status": {}, "scores": {"home_penalties": 3, "away_penalties": 2}}""";
        Assert.True(FlashScoreMapper.ShouldRequestPenaltiesEndpoint(json));
    }

    [Fact]
    public void ShouldRequestPenaltiesEndpoint_false_for_plain_finished_regulation()
    {
        Assert.False(FlashScoreMapper.ShouldRequestPenaltiesEndpoint(FinishedRegulationJson));
    }

    [Fact]
    public void Map_in_progress_with_extra_time_field_classifies_extra_time()
    {
        const string json = """
        {
          "match_id": "ET1",
          "match_status": {
            "is_started": true,
            "is_in_progress": true,
            "is_finished": false,
            "is_finished_after_extra_time": false,
            "is_finished_after_penalties": false
          },
          "timestamp": 1700000000,
          "scores": {
            "home_total": 2, "away_total": 2,
            "home_1st_half": 1, "away_1st_half": 1,
            "home_2nd_half": 1, "away_2nd_half": 1,
            "home_extra_time": 0, "away_extra_time": 0
          }
        }
        """;

        var snap = FlashScoreMapper.Map(json);

        Assert.Equal(MatchLifecyclePhase.ExtraTimeFirstHalf, snap.Phase);
        Assert.True(snap.WentToExtraTime);
        Assert.False(snap.WentToPenaltyShootout);
    }

    [Fact]
    public void Map_in_progress_with_penalty_scores_classifies_shootout()
    {
        const string json = """
        {
          "match_id": "PEN1",
          "match_status": {
            "is_started": true,
            "is_in_progress": true,
            "is_finished": false,
            "is_finished_after_extra_time": false,
            "is_finished_after_penalties": false
          },
          "timestamp": 1700000000,
          "scores": {
            "home_total": 1, "away_total": 1,
            "home_1st_half": 0, "away_1st_half": 1,
            "home_2nd_half": 1, "away_2nd_half": 0,
            "home_extra_time": 0, "away_extra_time": 0,
            "home_penalties": 2, "away_penalties": 1
          }
        }
        """;

        var snap = FlashScoreMapper.Map(json);

        Assert.Equal(MatchLifecyclePhase.PenaltyShootoutInProgress, snap.Phase);
        Assert.True(snap.WentToPenaltyShootout);
    }

    [Fact]
    public void Map_extracts_home_and_away_team_badge_urls_when_present()
    {
        var snap = FlashScoreMapper.Map(FinishedRegulationJson);

        Assert.Equal("https://static.flashscore.com/res/image/data/G2BgO5Ar-tIT0KyhE.png", snap.HomeTeamImageUrl);
        Assert.Equal("https://static.flashscore.com/res/image/data/EcTGi086-tIT0KyhE.png", snap.HomeTeamImageUrlSmall);
        Assert.Equal("h8oAv4Ts", snap.HomeTeamFlashScoreId);
        Assert.Equal("/team/sevilla/h8oAv4Ts/", snap.HomeTeamFlashScoreUrl);

        Assert.Equal("https://static.flashscore.com/res/image/data/A7kHoxZA-ttfpEDUq.png", snap.AwayTeamImageUrl);
        Assert.Equal("https://static.flashscore.com/res/image/data/CGPhnpne-ttfpEDUq.png", snap.AwayTeamImageUrlSmall);
        Assert.Equal("W8mj7MDD", snap.AwayTeamFlashScoreId);
        Assert.Equal("/team/real-madrid/W8mj7MDD/", snap.AwayTeamFlashScoreUrl);
    }

    [Fact]
    public void Map_leaves_team_badge_urls_null_when_team_blocks_absent()
    {
        const string json = """
        {
          "match_id": "NOTEAM",
          "match_status": {"is_finished": true},
          "timestamp": 1700000000,
          "scores": {"home_total": 1, "away_total": 0, "home_1st_half": 1, "away_1st_half": 0, "home_2nd_half": 0, "away_2nd_half": 0}
        }
        """;

        var snap = FlashScoreMapper.Map(json);

        Assert.Null(snap.HomeTeamImageUrl);
        Assert.Null(snap.AwayTeamImageUrl);
        Assert.Null(snap.HomeTeamFlashScoreId);
        Assert.Null(snap.AwayTeamFlashScoreUrl);
    }

    [Fact]
    public void ExtractRegularTimeGoals_falls_back_to_half_sum_when_totals_missing()
    {
        var scores = System.Text.Json.Nodes.JsonNode.Parse(
            """{"home_1st_half": 1, "home_2nd_half": 2, "away_1st_half": 0, "away_2nd_half": 1}""")!
            .AsObject();

        var (home, away, reliable) = FlashScoreMapper.ExtractRegularTimeGoals(
            scores, MatchLifecyclePhase.FinishedRegulation);

        Assert.Equal(3, home);
        Assert.Equal(1, away);
        Assert.True(reliable);
    }
}
