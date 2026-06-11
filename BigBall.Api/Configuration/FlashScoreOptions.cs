namespace BigBall.Api.Configuration;

public sealed class FlashScoreOptions
{
    public const string SectionName = "FlashScore";

    /// <summary>RapidAPI base URL for the FlashScore endpoints.</summary>
    public string BaseUrl { get; set; } = "https://flashscore4.p.rapidapi.com";

    /// <summary>RapidAPI key sent as <c>X-RapidAPI-Key</c>. Configure via user secrets or env vars.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>RapidAPI host sent as <c>X-RapidAPI-Host</c>; must match the BaseUrl host registered on RapidAPI.</summary>
    public string RapidApiHost { get; set; } = "flashscore4.p.rapidapi.com";
}
