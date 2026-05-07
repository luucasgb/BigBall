namespace BigBall.Api.Configuration;

/// <summary>Which adapter implementation feeds <see cref="BigBall.Domain.SportsData.ISportsDataSource"/>.</summary>
public sealed class SportsDataOptions
{
    public const string SectionName = "SportsData";

    /// <inheritdoc cref="SportsDataProviderNames"/>
    public string Provider { get; set; } = SportsDataProviderNames.SportsApiPro;
}
