using System.Globalization;
using System.Net;
using System.Text.Json.Nodes;
using BigBall.Domain.SportsData;

namespace BigBall.Api.Integrations.FlashScore;

/// <summary>FlashScore (RapidAPI) REST adapter implementing <see cref="ISportsDataSource"/> (TechSpec §6.2.2).</summary>
public sealed class FlashScoreSportsDataSource(HttpClient http) : ISportsDataSource
{
    private const string DetailsPath = "api/flashscore/v2/matches/details";
    private const string PenaltiesPath = "api/flashscore/v2/matches/penalties";
    private const string ListByDatePath = "api/flashscore/v2/matches/list-by-date";

    public async Task<SportsMatchSnapshot?> GetMatchByExternalIdAsync(
        string externalMatchId,
        SportsMatchFetchTelemetry? telemetry,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalMatchId))
            return null;

        var encoded = Uri.EscapeDataString(externalMatchId);

        using var detailRes = await http
            .GetAsync($"{DetailsPath}?match_id={encoded}", cancellationToken)
            .ConfigureAwait(false);

        if (detailRes.StatusCode == HttpStatusCode.NotFound)
            return null;

        detailRes.EnsureSuccessStatusCode();
        telemetry?.AddHttpGets(1);

        var detailsJson = await detailRes.Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        string? pensJson = null;
        if (FlashScoreMapper.ShouldRequestPenaltiesEndpoint(detailsJson))
        {
            using var pensRes = await http
                .GetAsync($"{PenaltiesPath}?match_id={encoded}", cancellationToken)
                .ConfigureAwait(false);

            if (pensRes.IsSuccessStatusCode)
            {
                telemetry?.AddHttpGets(1);
                pensJson = await pensRes.Content
                    .ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return FlashScoreMapper.Map(detailsJson, pensJson);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SportsScheduledFixture>> GetScheduleAsync(
        DateOnly date,
        SportsMatchFetchTelemetry? telemetry,
        CancellationToken cancellationToken = default)
    {
        var d = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        using var res = await http
            .GetAsync($"{ListByDatePath}?sport_id=1&date={d}", cancellationToken)
            .ConfigureAwait(false);

        if (!res.IsSuccessStatusCode)
            return Array.Empty<SportsScheduledFixture>();

        telemetry?.AddHttpGets(1);

        var json = await res.Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        var root = JsonNode.Parse(json);
        if (root is null)
            return Array.Empty<SportsScheduledFixture>();

        // Endpoint returns either an array of tournament groups (each with a "matches" array)
        // or already a flat "matches" array — accept both shapes.
        var tournaments = root as JsonArray ?? root["tournaments"] as JsonArray;
        var matchesArr = root["matches"] as JsonArray;

        var list = new List<SportsScheduledFixture>();

        if (tournaments is not null)
        {
            foreach (var t in tournaments)
            {
                if (t?["matches"] is not JsonArray inner)
                    continue;

                foreach (var item in inner)
                    if (ParseScheduleRow(item) is { } row)
                        list.Add(row);
            }
        }

        if (matchesArr is not null)
        {
            foreach (var item in matchesArr)
                if (ParseScheduleRow(item) is { } row)
                    list.Add(row);
        }

        return list;
    }

    private static SportsScheduledFixture? ParseScheduleRow(JsonNode? node)
    {
        if (node is null)
            return null;

        var externalId = node["match_id"]?.ToString();
        if (string.IsNullOrEmpty(externalId))
            return null;

        if (node["timestamp"] is not JsonValue tsVal
            || !tsVal.TryGetValue<long>(out var sec))
        {
            if (node["timestamp"]?.ToString() is not { } tsTxt
                || !long.TryParse(tsTxt, NumberStyles.Integer, CultureInfo.InvariantCulture, out sec))
                return null;
        }

        var kickoff = DateTimeOffset.FromUnixTimeSeconds(sec).UtcDateTime;

        var home = node["home_team"]?["short_name"]?.ToString()
                   ?? node["home_team"]?["name"]?.ToString();

        var away = node["away_team"]?["short_name"]?.ToString()
                   ?? node["away_team"]?["name"]?.ToString();

        return new SportsScheduledFixture(externalId, kickoff, home, away);
    }
}
