using BigBall.Api.Integrations.FlashScore;

namespace BigBall.Api.Tests.Integrations;

public sealed class FlashScoreTeamSearchServiceTests
{
    // Trimmed copy of the live RapidAPI `Search?q=brazil national team` payload — includes the
    // senior men's Brazil row plus several variants that the picker must reject (U21, U20, U23, W, Ol., basketball).
    private const string BrazilSearchPayload = """
    [
      {"id":"I9l9aqLq","type":"team","name":"Brazil","url":"brazil","sport":{"id":1,"name":"Soccer"},"gender":"Men","country_name":"South America","image_path":"https://static.flashscore.com/res/image/data/bymfq693-88LAtdNt.png","country_image_path":"https://static.flashscore.com/res/image/data/d8SjjLRN-lvob3Ero.png"},
      {"id":"OzVM8037","type":"team","name":"Team Durant","url":"team-durant","sport":{"id":3,"name":"Basketball"},"gender":"Men","country_name":"USA","image_path":"https://static.flashscore.com/res/image/data/Cppniwmh-nJfMvQpF.png"},
      {"id":"f1BgtqZs","type":"team","name":"Brazil U21","url":"brazil","sport":{"id":1,"name":"Soccer"},"gender":"Men","country_name":"South America","image_path":"https://static.flashscore.com/res/image/data/UBQsmyj9-bHI0hvMT.png"},
      {"id":"QiTr4PVe","type":"team","name":"Brazil U20","url":"brazil","sport":{"id":1,"name":"Soccer"},"gender":"Men","country_name":"South America","image_path":"https://static.flashscore.com/res/image/data/6Hx1rol9-vBKZiE2j.png"},
      {"id":"UZSq2YAO","type":"team","name":"Brazil W","url":"brazil","sport":{"id":1,"name":"Soccer"},"gender":"Women","country_name":"South America","image_path":"https://static.flashscore.com/res/image/data/Iq2XqgSc-vBKZiE2j.png"},
      {"id":"GQP4iGU0","type":"team","name":"Brazil Ol.","url":"brazil","sport":{"id":1,"name":"Soccer"},"gender":"Men","country_name":"South America","image_path":"https://static.flashscore.com/res/image/data/G0DHr9l9-GYIwiYHd.png"},
      {"id":"hCeTxXpO","type":"team","name":"Brazil U23","url":"brazil","sport":{"id":1,"name":"Soccer"},"gender":"Men","country_name":"South America","image_path":"https://static.flashscore.com/res/image/data/hbcaSV93-rVsp3mPF.png"}
    ]
    """;

    [Fact]
    public void PickBestTeamMatch_picks_senior_mens_brazil_over_age_groups_women_olympics_and_basketball()
    {
        var pick = FlashScoreTeamSearchService.PickBestTeamMatch(BrazilSearchPayload, "Brazil");

        Assert.NotNull(pick);
        Assert.Equal("Brazil", pick!.Name);
        Assert.Equal("I9l9aqLq", pick.TeamId);
        Assert.Equal("brazil", pick.TeamUrl);
        Assert.Equal("https://static.flashscore.com/res/image/data/bymfq693-88LAtdNt.png", pick.ImageUrl);
        Assert.Equal("https://static.flashscore.com/res/image/data/d8SjjLRN-lvob3Ero.png", pick.CountryImageUrl);
    }

    [Fact]
    public void PickBestTeamMatch_returns_null_when_payload_is_empty()
    {
        Assert.Null(FlashScoreTeamSearchService.PickBestTeamMatch("[]", "Brazil"));
        Assert.Null(FlashScoreTeamSearchService.PickBestTeamMatch("", "Brazil"));
    }

    [Fact]
    public void PickBestTeamMatch_returns_null_when_only_excluded_variants_present()
    {
        const string json = """
        [
          {"id":"x","type":"team","name":"Brazil U21","url":"brazil","sport":{"id":1,"name":"Soccer"},"gender":"Men","image_path":"https://x/y.png"},
          {"id":"y","type":"team","name":"Brazil W","url":"brazil","sport":{"id":1,"name":"Soccer"},"gender":"Women","image_path":"https://x/y.png"}
        ]
        """;

        Assert.Null(FlashScoreTeamSearchService.PickBestTeamMatch(json, "Brazil"));
    }

    [Fact]
    public void PickBestTeamMatch_prefers_exact_name_over_partial_match()
    {
        const string json = """
        [
          {"id":"first","type":"team","name":"Brazilian Soccer","url":"x","sport":{"id":1,"name":"Soccer"},"gender":"Men","image_path":"https://x/a.png"},
          {"id":"second","type":"team","name":"Brazil","url":"brazil","sport":{"id":1,"name":"Soccer"},"gender":"Men","image_path":"https://x/b.png"}
        ]
        """;

        var pick = FlashScoreTeamSearchService.PickBestTeamMatch(json, "Brazil");

        Assert.NotNull(pick);
        Assert.Equal("second", pick!.TeamId);
    }

    [Fact]
    public void PickBestTeamMatch_skips_entries_without_an_image_path()
    {
        const string json = """
        [
          {"id":"a","type":"team","name":"Brazil","url":"brazil","sport":{"id":1,"name":"Soccer"},"gender":"Men"},
          {"id":"b","type":"team","name":"Brazil","url":"brazil","sport":{"id":1,"name":"Soccer"},"gender":"Men","image_path":"https://x/b.png"}
        ]
        """;

        var pick = FlashScoreTeamSearchService.PickBestTeamMatch(json, "Brazil");

        Assert.NotNull(pick);
        Assert.Equal("b", pick!.TeamId);
    }
}
