namespace BigBall.Api.Configuration;

public sealed class SportsApiProOptions
{
    public const string SectionName = "SportsApiPro";

    /// <summary>
    /// REST base URL for Football V2 (see SportsAPI Pro docs).
    /// </summary>
    public string BaseUrl { get; set; } = "https://v2.football.sportsapipro.com";

    /// <summary>
    /// API key sent as <c>x-api-key</c>. Set via user secrets (Development) or environment variables.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
