using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using BigBall.Shared.Dtos;
using Microsoft.Extensions.Logging;

namespace BigBall.Api.Integrations.SportsApiPro;

/// <summary>
/// Development helper: loads FIFA World Cup schedule via Football V2 (canonical <c>uniqueTournament.id</c> = 16).
/// Resolves the active season with <c>GET api/tournaments/{id}</c> first (canonical id is stable across seasons);
/// falls back to <c>/seasons</c> if the detail payload has no ids. Then fetches rounds and events.
/// One run performs many upstream HTTP calls — use sparingly.
/// </summary>
public sealed class SportsApiProWorldCup2026ProbeService(
    HttpClient http,
    ILogger<SportsApiProWorldCup2026ProbeService> logger)
{
    public const int CanonicalWorldCupUniqueTournamentId = 16;

    private static readonly JsonSerializerOptions LogJson = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Maximum characters written to <see cref="WorldCup2026SportsApiProbeDto.AggregateJsonPreview"/>.</summary>
    private const int PreviewCap = 4000;

    public async Task<WorldCup2026SportsApiProbeDto> RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var resolved = await ResolveVendorSeasonAsync(cancellationToken).ConfigureAwait(false);
            if (!resolved.Ok)
                return Fail(resolved.Error ?? "Could not resolve vendor season.");

            var seasonId = resolved.SeasonId!.Value;
            var tournamentId = resolved.TournamentId!.Value;
            var seasonLabel = resolved.SeasonLabel;

            using var roundsRes = await http
                .GetAsync($"api/tournament/{tournamentId}/season/{seasonId}/rounds", cancellationToken)
                .ConfigureAwait(false);

            var roundsRaw = await roundsRes.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!roundsRes.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "WorldCup2026 probe: rounds failed {Status} tournamentId={TournamentId} seasonId={SeasonId} Body={Body}",
                    (int)roundsRes.StatusCode,
                    tournamentId,
                    seasonId,
                    roundsRaw);
                return Fail($"SportsApi rounds HTTP {(int)roundsRes.StatusCode}. See server logs for full body.");
            }

            var roundsRoot = JsonNode.Parse(roundsRaw);
            if (!TryGetRoundsArray(roundsRoot, out var roundsArr) || roundsArr!.Count == 0)
            {
                logger.LogWarning("WorldCup2026 probe: no rounds array for tournamentId={Tid} seasonId={Sid}", tournamentId, seasonId);
                return Fail("SportsApi rounds: empty or unrecognized structure.");
            }

            var byEventId = new SortedDictionary<long, JsonNode>(Comparer<long>.Create((a, b) => a.CompareTo(b)));
            var roundKeys = new List<string>(roundsArr.Count);
            foreach (var rnode in roundsArr)
            {
                if (rnode is null)
                    continue;
                var key = ExtractRoundPathSegment(rnode);
                if (key is not null)
                    roundKeys.Add(key);
            }

            if (roundKeys.Count == 0)
                return Fail("Could not derive round identifiers from /rounds response.");

            foreach (var roundKey in roundKeys.Distinct(StringComparer.Ordinal))
            {
                var path =
                    $"api/tournament/{tournamentId}/season/{seasonId}/round/{Uri.EscapeDataString(roundKey)}";
                using var roundRes = await http.GetAsync(path, cancellationToken).ConfigureAwait(false);
                var roundBody = await roundRes.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                if (roundRes.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.LogInformation(
                        "WorldCup2026 probe: round {Round} returned 404 (skipped).",
                        roundKey);
                    continue;
                }

                if (!roundRes.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "WorldCup2026 probe: round {Round} HTTP {Status} Body={Body}",
                        roundKey,
                        (int)roundRes.StatusCode,
                        roundBody);
                    continue;
                }

                var roundJson = JsonNode.Parse(roundBody);
                MergeEventsFromRound(byEventId, roundJson);
            }

            var aggregate = new JsonObject
            {
                ["uniqueTournamentId"] = CanonicalWorldCupUniqueTournamentId,
                ["vendorTournamentId"] = tournamentId,
                ["vendorSeasonId"] = seasonId,
                ["seasonLabel"] = seasonLabel ?? "",
                ["events"] = new JsonArray(byEventId.Values.ToArray()),
            };

            var aggregateText = aggregate.ToJsonString(LogJson);
            var previewLen = Math.Min(PreviewCap, aggregateText.Length);
            var preview = aggregateText[..previewLen];

            logger.LogInformation(
                "WorldCup2026 probe OK: Season={Season} VendorSeasonId={SeasonId} VendorTournamentId={TournamentId} RoundsRequested={Rounds} UniqueEvents={Count} AggregateChars={Chars}",
                seasonLabel,
                seasonId,
                tournamentId,
                roundKeys.Distinct(StringComparer.Ordinal).Count(),
                byEventId.Count,
                aggregateText.Length);

            logger.LogInformation(
                "WorldCup2026 probe aggregate preview (first {PreviewLen} chars): {Preview}",
                previewLen,
                preview);

            return new WorldCup2026SportsApiProbeDto(
                Success: true,
                Error: null,
                TotalUniqueMatches: byEventId.Count,
                RoundsQueried: roundKeys.Count,
                SeasonLabel: seasonLabel,
                VendorSeasonId: seasonId,
                VendorTournamentId: tournamentId,
                AggregateJsonLength: aggregateText.Length,
                AggregateJsonPreview: preview);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "WorldCup2026 probe failed with exception.");
            return Fail($"Probe error: {ex.Message}");
        }
    }

    private async Task<(bool Ok, long? SeasonId, long? TournamentId, string? SeasonLabel, string? Error)>
        ResolveVendorSeasonAsync(CancellationToken cancellationToken)
    {
        var detailPath = $"api/tournaments/{CanonicalWorldCupUniqueTournamentId}";
        using var detailRes = await http.GetAsync(detailPath, cancellationToken).ConfigureAwait(false);
        var detailRaw = await detailRes.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (detailRes.IsSuccessStatusCode)
        {
            var root = JsonNode.Parse(detailRaw);
            var node = UnwrapApiEnvelope(root);
            if (TryExtractSeasonContext(node, out var sid, out var tid, out var detailLabel))
            {
                logger.LogInformation(
                    "WorldCup2026 probe: season from tournament detail {Path} VendorSeasonId={SeasonId} VendorTournamentId={TournamentId}",
                    detailPath,
                    sid,
                    tid);
                return (true, sid, tid, detailLabel, null);
            }

            logger.LogWarning(
                "WorldCup2026 probe: tournament detail OK but season ids not found in JSON. Preview={Preview}",
                TruncateForLog(detailRaw, 2000));
        }
        else
        {
            logger.LogWarning(
                "WorldCup2026 probe: tournament detail HTTP {Status} Uri={Uri} Body={Body}",
                (int)detailRes.StatusCode,
                detailRes.RequestMessage?.RequestUri?.ToString()
                ?? new Uri(http.BaseAddress!, detailPath).ToString(),
                detailRaw);
        }

        var seasonsPath = $"api/tournaments/{CanonicalWorldCupUniqueTournamentId}/seasons";
        using var seasonsRes = await http.GetAsync(seasonsPath, cancellationToken).ConfigureAwait(false);
        var seasonsRaw = await seasonsRes.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!seasonsRes.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "WorldCup2026 probe: seasons fallback HTTP {Status} Uri={Uri} Body={Body}",
                (int)seasonsRes.StatusCode,
                seasonsRes.RequestMessage?.RequestUri?.ToString()
                ?? new Uri(http.BaseAddress!, seasonsPath).ToString(),
                seasonsRaw);
            return (
                false,
                null,
                null,
                null,
                "Could not resolve season via api/tournaments/{id} or /seasons. See server logs for response bodies.");
        }

        var seasonsRoot = JsonNode.Parse(seasonsRaw);
        if (seasonsRoot is null)
            return (false, null, null, null, "Seasons fallback: empty JSON.");

        if (!TryGetSeasonsArray(seasonsRoot, out var seasonsArr) || seasonsArr.Count == 0)
            return (false, null, null, null, "Seasons fallback: no seasons array.");

        if (!TryPickPreferredSeason(seasonsArr, out var seasonNode, out var pickReason))
            return (false, null, null, null, pickReason);

        var seasonIdFb = ParseLong(seasonNode!["id"]);
        var tournamentIdFb = ParseLong(seasonNode["tournament"]?["id"]);
        var fallbackLabel = BuildSeasonLabel(seasonNode);
        if (seasonIdFb is null || tournamentIdFb is null)
            return (false, null, null, null, "Seasons fallback: missing tournament.id or season id.");

        logger.LogInformation("WorldCup2026 probe: season from {Path} (fallback)", seasonsPath);
        return (true, seasonIdFb, tournamentIdFb, fallbackLabel, null);
    }

    private static JsonNode? UnwrapApiEnvelope(JsonNode? root)
    {
        if (root is null)
            return null;

        if (root["data"] is JsonObject or JsonArray)
            return root["data"];

        return root;
    }

    /// <summary>
    /// Extracts season-scoped <c>tournament.id</c> and <c>season.id</c> from a tournament-detail or similar payload.
    /// </summary>
    private static bool TryExtractSeasonContext(
        JsonNode? node,
        out long? seasonId,
        out long? tournamentId,
        out string? seasonLabel)
    {
        seasonId = null;
        tournamentId = null;
        seasonLabel = null;
        if (node is null)
            return false;

        if (TryPairSeasonAndTournament(node["season"], node["tournament"], out seasonId, out tournamentId, out seasonLabel))
            return true;

        if (node["currentSeason"] is JsonObject cur &&
            TryPairSeasonAndTournament(cur, cur["tournament"] ?? node["tournament"], out seasonId, out tournamentId, out seasonLabel))
            return true;

        if (node["primarySeason"] is JsonObject prim &&
            TryPairSeasonAndTournament(prim, prim["tournament"] ?? node["tournament"], out seasonId, out tournamentId, out seasonLabel))
            return true;

        if (node["tournament"] is JsonObject tobj)
        {
            var s = tobj["season"] ?? node["season"];
            if (TryPairSeasonAndTournament(s, tobj, out seasonId, out tournamentId, out seasonLabel))
                return true;
        }

        if (node["seasons"] is JsonArray ja &&
            TryPickPreferredSeason(ja, out var pick, out _) &&
            pick is not null &&
            TryPairSeasonAndTournament(pick, pick["tournament"], out seasonId, out tournamentId, out seasonLabel))
            return true;

        if (ParseLong(node["seasonId"]) is { } flatSid && ParseLong(node["tournamentId"]) is { } flatTid)
        {
            seasonId = flatSid;
            tournamentId = flatTid;
            seasonLabel = node["seasonName"]?.ToString() ?? node["name"]?.ToString();
            return true;
        }

        return false;
    }

    private static bool TryPairSeasonAndTournament(
        JsonNode? seasonObj,
        JsonNode? tournamentObj,
        out long? seasonId,
        out long? tournamentId,
        out string? seasonLabel)
    {
        seasonId = ParseLong(seasonObj?["id"]);
        tournamentId = ParseLong(tournamentObj?["id"]);
        seasonLabel = BuildSeasonLabel(seasonObj);
        return seasonId is not null && tournamentId is not null;
    }

    private static string? BuildSeasonLabel(JsonNode? seasonNode)
    {
        if (seasonNode is null)
            return null;

        var seasonName = seasonNode["name"]?.ToString();
        var year = seasonNode["year"]?.ToString();
        if (string.IsNullOrWhiteSpace(seasonName))
            return string.IsNullOrWhiteSpace(year) ? null : year;
        return string.IsNullOrWhiteSpace(year) ? seasonName : $"{seasonName} ({year})";
    }

    private static string TruncateForLog(string s, int maxChars) =>
        s.Length <= maxChars ? s : s[..maxChars] + "…";

    private static WorldCup2026SportsApiProbeDto Fail(string error) =>
        new(
            Success: false,
            Error: error,
            TotalUniqueMatches: 0,
            RoundsQueried: 0,
            SeasonLabel: null,
            VendorSeasonId: null,
            VendorTournamentId: null,
            AggregateJsonLength: 0,
            AggregateJsonPreview: null);

    private static bool TryGetSeasonsArray(JsonNode root, out JsonArray arr)
    {
        arr = root["seasons"] as JsonArray
              ?? root["data"]?["seasons"] as JsonArray
              ?? (root["data"] as JsonObject)?["seasons"] as JsonArray
              ?? (root["data"] as JsonArray)
              ?? null!;
        return arr is not null && arr.Count > 0;
    }

    /// <summary>
    /// Prefer the vendor "current" season, then the highest numeric <c>id</c> (usually latest),
    /// then the first entry — matches typical SportsAPI "current cycle" data without hard-coding a year.
    /// </summary>
    private static bool TryPickPreferredSeason(JsonArray seasons, out JsonNode? season, out string reason)
    {
        reason = string.Empty;
        season = null;

        foreach (var s in seasons)
        {
            if (s is null)
                continue;
            if (JsonTruthy(s["current"]) || JsonTruthy(s["isCurrent"]))
            {
                season = s;
                return true;
            }
        }

        JsonNode? maxIdSeason = null;
        long maxId = long.MinValue;
        foreach (var s in seasons)
        {
            if (s is null)
                continue;
            var id = ParseLong(s["id"]);
            if (id is { } v && v > maxId)
            {
                maxId = v;
                maxIdSeason = s;
            }
        }

        if (maxIdSeason is not null)
        {
            season = maxIdSeason;
            return true;
        }

        season = seasons.FirstOrDefault(static n => n is not null);
        if (season is not null)
            return true;

        reason = "SportsApi seasons array had no usable entries.";
        return false;
    }

    private static bool JsonTruthy(JsonNode? n)
        => n switch
        {
            JsonValue v when v.TryGetValue<bool>(out var b) => b,
            JsonValue v when v.TryGetValue<int>(out var i) => i != 0,
            JsonValue sv when string.Equals(sv.ToString(), "true", StringComparison.OrdinalIgnoreCase) => true,
            _ => false,
        };

    private static bool TryGetRoundsArray(JsonNode? root, out JsonArray? arr)
    {
        arr = root?["rounds"] as JsonArray
              ?? root?["data"]?["rounds"] as JsonArray
              ?? (root?["data"] as JsonObject)?["rounds"] as JsonArray
              ?? root?["data"] as JsonArray;
        return arr is not null && arr.Count > 0;
    }

    /// <summary>Path segment for GET .../round/{segment}.</summary>
    private static string? ExtractRoundPathSegment(JsonNode roundNode)
    {
        var slug = roundNode["slug"]?.ToString()?.Trim();
        if (!string.IsNullOrEmpty(slug))
            return slug;

        if (ParseLong(roundNode["round"]) is { } r)
            return r.ToString(CultureInfo.InvariantCulture);
        if (ParseLong(roundNode["roundNumber"]) is { } rn)
            return rn.ToString(CultureInfo.InvariantCulture);
        if (ParseLong(roundNode["number"]) is { } num)
            return num.ToString(CultureInfo.InvariantCulture);

        return roundNode["id"]?.ToString()?.Trim();
    }

    private static void MergeEventsFromRound(IDictionary<long, JsonNode> byId, JsonNode? roundRoot)
    {
        if (roundRoot is null)
            return;

        TryAddEvents(byId, roundRoot["events"] as JsonArray);
        TryAddEvents(byId, roundRoot["games"] as JsonArray);
        TryAddEvents(byId, roundRoot["data"]?["events"] as JsonArray);
        TryAddEvents(byId, roundRoot["data"]?["games"] as JsonArray);

        if (roundRoot["data"] is JsonObject dataObj && dataObj["events"] is JsonArray nested)
            TryAddEvents(byId, nested);
    }

    private static void TryAddEvents(IDictionary<long, JsonNode> byId, JsonArray? arr)
    {
        if (arr is null)
            return;
        foreach (var item in arr)
        {
            if (item is null)
                continue;
            var id = ParseLong(item["id"]);
            if (id is null)
                continue;
            byId[id.Value] = item;
        }
    }

    private static long? ParseLong(JsonNode? n)
        => n switch
        {
            JsonValue v when v.TryGetValue<long>(out var lo) => lo,
            JsonValue sv when sv.ToString()?.Trim() is { } ts &&
                             long.TryParse(ts, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lx) => lx,
            _ => null,
        };
}
