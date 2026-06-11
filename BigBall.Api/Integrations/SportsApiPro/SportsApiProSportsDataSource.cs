using System.Globalization;
using System.Net;
using System.Text.Json.Nodes;
using BigBall.Domain.SportsData;

namespace BigBall.Api.Integrations.SportsApiPro;

/// <summary>Football V2 REST adapter implementing <see cref="ISportsDataSource"/>.</summary>
public sealed class SportsApiProSportsDataSource(HttpClient http) : ISportsDataSource
{
    public async Task<SportsMatchSnapshot?> GetMatchByExternalIdAsync(
        string externalMatchId,
        SportsMatchFetchTelemetry? telemetry,
        CancellationToken cancellationToken = default)
    {
        if (!long.TryParse(externalMatchId, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
            return null;

        using var detailRes =
            await http.GetAsync($"api/match/{id}", cancellationToken).ConfigureAwait(false);

        switch (detailRes.StatusCode)
        {
            case HttpStatusCode.NotFound:
                return null;
            default:
                detailRes.EnsureSuccessStatusCode();
                break;
        }

        telemetry?.AddHttpGets(1);
        var matchJson = await detailRes.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var statusCode = SportsApiProMapper.PeekMatchStatusCode(matchJson);

        string? scoresJson = null;
        if (SportsApiProMapper.ShouldRequestScoresEndpoint(matchJson, statusCode))
        {
            using var scoresRes =
                await http.GetAsync($"api/match/{id}/scores", cancellationToken).ConfigureAwait(false);
            if (scoresRes.IsSuccessStatusCode)
            {
                telemetry?.AddHttpGets(1);
                scoresJson = await scoresRes.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        string? pensJson = null;
        if (SportsApiProMapper.ShouldRequestPenaltiesEndpoint(statusCode))
        {
            using var pensRes =
                await http.GetAsync($"api/match/{id}/penalties", cancellationToken).ConfigureAwait(false);
            if (pensRes.IsSuccessStatusCode)
            {
                telemetry?.AddHttpGets(1);
                pensJson = await pensRes.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        return SportsApiProMapper.Map(matchJson, scoresJson, pensJson);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SportsScheduledFixture>> GetScheduleAsync(
        DateOnly date,
        SportsMatchFetchTelemetry? telemetry,
        CancellationToken cancellationToken = default)
    {
        var d = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        using var res = await http
            .GetAsync($"api/schedule/{d}", cancellationToken)
            .ConfigureAwait(false);

        if (!res.IsSuccessStatusCode)
            return Array.Empty<SportsScheduledFixture>();

        telemetry?.AddHttpGets(1);

        var json = await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var root = JsonNode.Parse(json);
        if (root is null)
            return Array.Empty<SportsScheduledFixture>();

        var arr = root["events"] as JsonArray
                  ?? root["data"]?.AsArray();

        if (arr is null || arr.Count == 0)
            return Array.Empty<SportsScheduledFixture>();

        var list = new List<SportsScheduledFixture>();

        foreach (var item in arr)
        {
            if (ParseScheduleRow(item) is { } row)
                list.Add(row);
        }

        return list;
    }

    private static SportsScheduledFixture? ParseScheduleRow(JsonNode? evt)
    {
        if (evt is null || IdToString(evt["id"]) is not { } extId)
            return null;

        if (ParseLong(evt["startTimestamp"]) is not long sec)
            return null;

        var kickoff = DateTimeOffset.FromUnixTimeSeconds(sec).UtcDateTime;

        var home = evt["homeTeam"]?.AsObject()?["shortName"]?.ToString()
                   ?? evt["homeTeam"]?.AsObject()?["name"]?.ToString()
                   ?? evt["homeTeam"]?["name"]?.ToString()
                   ?? evt["homeTeam"]?.ToString();

        var away = evt["awayTeam"]?.AsObject()?["shortName"]?.ToString()
                   ?? evt["awayTeam"]?.AsObject()?["name"]?.ToString()
                   ?? evt["awayTeam"]?["name"]?.ToString()
                   ?? evt["awayTeam"]?.ToString();

        return new SportsScheduledFixture(extId, kickoff, home, away);
    }

    private static string? IdToString(JsonNode? node)
        => node switch
        {
            JsonValue v when v.TryGetValue<long>(out var l) => l.ToString(CultureInfo.InvariantCulture),
            null => null,
            JsonValue xv when xv.ToString()?.Trim() is { } zs &&
                               long.TryParse(
                                   zs,
                                   NumberStyles.Integer,
                                   CultureInfo.InvariantCulture,
                                   out var id)
                =>
                id.ToString(CultureInfo.InvariantCulture),
            _ => node.ToString()
        };

    private static long? ParseLong(JsonNode? n)
        => n switch
        {
            JsonValue v when v.TryGetValue<long>(out var lo) => lo,
            JsonValue sv when sv.ToString()?.Trim() is { } ts &&
                                long.TryParse(
                                    ts,
                                    NumberStyles.Integer,
                                    CultureInfo.InvariantCulture,
                                    out var lx)
                =>
                lx,
            _ => null
        };
}
