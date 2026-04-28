using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using BigBall.Api.Configuration;
using BigBall.Domain.SportsData;

namespace BigBall.Api.Integrations.SportsApiPro;

/// <summary>Maps Football V2 JSON payloads to canonical <see cref="SportsMatchSnapshot"/> (Tech Spec §6.2).</summary>
public static class SportsApiProMapper
{
    public const string ProviderName = SportsDataProviderNames.SportsApiPro;

    /// <summary>
    /// Maps SportsAPI Pro Football V2 payloads (<c>/api/match/{{id}}</c>, optional <c>/scores</c>, <c>/penalties</c>)
    /// into the BigBall canonical snapshot.
    /// </summary>
    public static SportsMatchSnapshot Map(
        string matchJson,
        string? scoresJson = null,
        string? penaltiesJson = null)
    {
        var root = JsonNode.Parse(matchJson)
                     ?? throw new JsonException("Empty match payload.");

        var evt = ResolveEvent(root)
                  ?? throw new JsonException("Missing event.");

        var externalId = IdToString(evt["id"])
                         ?? throw new JsonException("Missing match id.");

        var kickoffUtc = ResolveKickoffUtc(evt);

        var statusCode = GetStatusCode(evt);
        var phase = MapLifecycleFromStatus(statusCode);

        MergeScoresIntoEvent(evt, scoresJson);

        JsonObject? hs = GetScoreBlock(evt, true);
        JsonObject? ash = GetScoreBlock(evt, false);

        var (regsH, regsA, trReliable) = ExtractRegularTimeGoals(hs, ash, phase);

        var wentEt = LifecycleImpliesExtraTime(phase);
        var wentPens = LifecycleImpliesPenalties(phase);

        if (!trReliable && ShouldFlagGapForTr(phase, hs, ash))
            trReliable = false;

        var winner = ResolvePenaltyWinner(evt, penaltiesJson);

        var origin = ResolveResultOrigin(trReliable, regsH, regsA);

        return new SportsMatchSnapshot(
            ProviderName,
            externalId,
            kickoffUtc,
            phase,
            statusCode,
            regsH,
            regsA,
            trReliable,
            wentEt,
            wentPens,
            winner,
            origin);
    }

    public static MatchLifecyclePhase MapLifecycleFromStatus(int? statusCode)
    {
        return statusCode switch
        {
            null => MatchLifecyclePhase.Unknown,
            0 => MatchLifecyclePhase.NotStarted,
            6 => MatchLifecyclePhase.FirstHalf,
            7 => MatchLifecyclePhase.SecondHalf,
            31 => MatchLifecyclePhase.Halftime,
            40 => MatchLifecyclePhase.ExtraTimeFirstHalf,
            41 => MatchLifecyclePhase.ExtraTimeSecondHalf,
            50 => MatchLifecyclePhase.PenaltyShootoutInProgress,
            60 => MatchLifecyclePhase.Postponed,
            70 => MatchLifecyclePhase.Canceled,
            80 => MatchLifecyclePhase.Interrupted,
            90 => MatchLifecyclePhase.Abandoned,
            100 => MatchLifecyclePhase.FinishedRegulation,
            110 => MatchLifecyclePhase.FinishedAfterExtraTime,
            120 => MatchLifecyclePhase.FinishedAfterPenalties,
            _ => MatchLifecyclePhase.Unknown
        };
    }

    /// <summary>Peek status code from raw <c>GET /api/match/{id}</c> payload without auxiliary endpoints.</summary>
    public static int? PeekMatchStatusCode(string matchJson)
    {
        if (string.IsNullOrWhiteSpace(matchJson))
            return null;

        var root = JsonNode.Parse(matchJson);
        if (root is null)
            return null;

        var evt = ResolveEvent(root);
        return evt is null ? null : GetStatusCode(evt);
    }

    /// <summary>Whether to call <c>GET …/scores</c> — heuristic to avoid redundant auxiliary calls.</summary>
    public static bool ShouldRequestScoresEndpoint(string matchJson, int? statusCode)
    {
        var phase = MapLifecycleFromStatus(statusCode);
        if (phase == MatchLifecyclePhase.NotStarted || phase == MatchLifecyclePhase.Unknown && statusCode is null)
            return false;

        var root = JsonNode.Parse(matchJson);
        var evt = root is null ? null : ResolveEvent(root);
        if (evt is null)
            return true;

        JsonObject? hs = GetScoreBlock(evt, true);
        JsonObject? ash = GetScoreBlock(evt, false);
        var (regsH, regsA, _) = ExtractRegularTimeGoals(hs, ash, phase);
        var hasNt = regsH is not null && regsA is not null;

        if (hasNt &&
            LifecycleImpliesExtraTimeOrPens(phase) &&
            ShouldFlagGapForTr(phase, hs, ash))
            return true;

        if (!hasNt)
            return true;

        return LifecycleImpliesExtraTimeOrPens(phase) && ShouldFlagGapForTr(phase, hs, ash);
    }

    /// <summary>
    /// Whether to call <c>GET …/penalties</c> — only when the lifecycle needs penalty aggregates not reliably in the match JSON.
    /// </summary>
    public static bool ShouldRequestPenaltiesEndpoint(int? statusCode)
    {
        return statusCode is 50 or 120;
    }

    private static SportsResultOrigin ResolveResultOrigin(
        bool trReliable,
        int? regsH,
        int? regsA)
    {
        if (trReliable && regsH is not null && regsA is not null)
            return SportsResultOrigin.ProviderComplete;

        if (regsH is null && regsA is null)
            return SportsResultOrigin.Unknown;

        return trReliable ? SportsResultOrigin.ProviderPartial : SportsResultOrigin.GapRegularTimeUnresolved;
    }

    private static JsonNode? ResolveEvent(JsonNode root)
        => root["event"] ??
           root["data"] ??
           (root.AsObject()?.ContainsKey("startTimestamp") == true ||
            root.AsObject()?.ContainsKey("id") == true
               ? root
               : null);

    private static void MergeScoresIntoEvent(JsonNode evt, string? scoresJson)
    {
        if (string.IsNullOrEmpty(scoresJson))
            return;

        var extra = JsonNode.Parse(scoresJson);
        if (extra is null)
            return;

        var graft = extra["scores"] ?? extra["event"] ?? extra["data"] ?? extra;
        if (graft is not JsonObject o)
            return;

        foreach (var kv in o)
            evt[kv.Key] = kv.Value?.DeepClone();
    }

    private static JsonObject? GetScoreBlock(JsonNode evt, bool home)
    {
        var key = home ? "homeScore" : "awayScore";

        switch (evt[key])
        {
            case JsonObject o:
                return o;
        }

        var altSide = evt[home ? "home" : "away"];
        switch (altSide?["score"])
        {
            case JsonObject nested:
                return nested;
            default:
                return null;
        }
    }

    private static DateTime ResolveKickoffUtc(JsonNode evt)
    {
        if (ParseLongTry(evt["startTimestamp"]) is not long sec)
            throw new JsonException("Missing startTimestamp.");

        return DateTimeOffset.FromUnixTimeSeconds(sec).UtcDateTime;
    }

    private static int? GetStatusCode(JsonNode evt)
        => AsInt(evt["status"]?["statusCode"] ??
                 evt["status"]?["code"] ??
                 evt["status"]);

    private static (int? regsH, int? regsA, bool reliable) ExtractRegularTimeGoals(
        JsonObject? homeScore,
        JsonObject? awayScore,
        MatchLifecyclePhase phase)
    {
        var ntH = AsInt(homeScore?["normaltime"]);
        var ntA = AsInt(awayScore?["normaltime"]);
        if (ntH.HasValue || ntA.HasValue)
            return (ntH, ntA, ntH.HasValue && ntA.HasValue);

        var p1H = AsInt(homeScore?["period1"]);
        var p2H = AsInt(homeScore?["period2"]);
        var p1A = AsInt(awayScore?["period1"]);
        var p2A = AsInt(awayScore?["period2"]);

        if (p1H.HasValue && p2H.HasValue && p1A.HasValue && p2A.HasValue)
        {
            var sumH = p1H!.Value + p2H!.Value;
            var sumA = p1A!.Value + p2A!.Value;
            var reliable = phase == MatchLifecyclePhase.FinishedRegulation ||
                           phase == MatchLifecyclePhase.Unknown ||
                           phase <= MatchLifecyclePhase.Halftime;
            return (sumH, sumA, reliable && !LifecycleImpliesExtraTimeOrPens(phase));
        }

        var dH = AsInt(homeScore?["display"] ?? homeScore?["current"]);
        var dA = AsInt(awayScore?["display"] ?? awayScore?["current"]);

        var fallbackOk = phase is MatchLifecyclePhase.FinishedRegulation
            or MatchLifecyclePhase.NotStarted
            or MatchLifecyclePhase.FirstHalf
            or MatchLifecyclePhase.SecondHalf
            or MatchLifecyclePhase.Halftime;

        return (dH, dA, fallbackOk && (!LifecycleImpliesExtraTimeOrPens(phase)));
    }

    private static bool ShouldFlagGapForTr(MatchLifecyclePhase phase, JsonObject? hs, JsonObject? ash)
    {
        if (!LifecycleImpliesExtraTimeOrPens(phase))
            return false;

        return !(AsInt(hs?["normaltime"]).HasValue && AsInt(ash?["normaltime"]).HasValue);
    }

    private static bool LifecycleImpliesExtraTimeOrPens(MatchLifecyclePhase phase) =>
        phase is MatchLifecyclePhase.ExtraTimeFirstHalf
               or MatchLifecyclePhase.ExtraTimeSecondHalf
               or MatchLifecyclePhase.FinishedAfterExtraTime
               or MatchLifecyclePhase.PenaltyShootoutInProgress
               or MatchLifecyclePhase.FinishedAfterPenalties;

    private static bool LifecycleImpliesExtraTime(MatchLifecyclePhase phase) =>
        phase is MatchLifecyclePhase.ExtraTimeFirstHalf
               or MatchLifecyclePhase.ExtraTimeSecondHalf
               or MatchLifecyclePhase.FinishedAfterExtraTime
               or MatchLifecyclePhase.PenaltyShootoutInProgress
               or MatchLifecyclePhase.FinishedAfterPenalties;

    private static bool LifecycleImpliesPenalties(MatchLifecyclePhase phase) =>
        phase is MatchLifecyclePhase.PenaltyShootoutInProgress
               or MatchLifecyclePhase.FinishedAfterPenalties;

    private static bool? ResolvePenaltyWinner(JsonNode evt, string? penaltiesJson)
    {
        if (string.IsNullOrWhiteSpace(penaltiesJson))
            return null;

        var doc = JsonNode.Parse(penaltiesJson);
        if (doc is null)
            return null;

        var sum = doc["summary"] ?? doc["statistics"] ?? doc;
        if (sum["homePenaltyScore"] is JsonObject or JsonValue ||
            sum["awayPenaltyScore"] is JsonObject or JsonValue ||
            doc["homePenaltyScore"] != null)
        {
            var hPen = FirstInt(sum, "homePenaltyScore", "homePenalties") ?? FirstInt(doc, "homePenaltyScore");
            var aPen = FirstInt(sum, "awayPenaltyScore", "awayPenalties") ?? FirstInt(doc, "awayPenaltyScore");

            if (hPen is {} hv && aPen is { } av && hv != av)
                return hv > av;
        }

        var hid = evt["homeTeam"]?["id"];
        long? hidL = hid is JsonValue hj && hj.TryGetValue<long>(out var lh) ? lh :
            hid?.ToString().Trim() is {} s &&
            long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
                ? v
                : null;

        long? aidL =
            evt["awayTeam"]?["id"] is JsonValue aj && aj.TryGetValue<long>(out var la)
                ? la
                : evt["awayTeam"]?["id"]?.ToString().Trim() is { } zs &&
                  long.TryParse(zs, NumberStyles.Integer, CultureInfo.InvariantCulture, out var zb)
                      ? zb
                      : null;

        if (hidL.HasValue &&
            ParseLongTry(doc["winnerTeamId"]) is { } wt)
        {
            if (wt == hidL.Value) return true;
            if (aidL.HasValue && wt == aidL.Value) return false;
        }

        switch (doc["winnerIsHomeTeam"])
        {
            case JsonValue wv when wv.TryGetValue<bool>(out var isHome):
                return isHome;
        }

        if (doc["winner"]?.AsObject() is { } wm &&
            wm.TryGetPropertyValue("winnerIsHome", out var wi) &&
            wi is JsonValue wix && wix.TryGetValue<bool>(out var isHm))
            return isHm;

        return null;
    }

    private static int? FirstInt(JsonNode? doc, params string[] names)
    {
        foreach (var n in names)
        {
            if (doc?[n] is JsonValue vv && vv.TryGetValue<long>(out var l))
                return (int)Math.Min(int.MaxValue, l);

            switch (doc?[n])
            {
                case JsonObject o when AsInt(o["score"]) is { } xi:
                    return xi;
                case JsonArray arr when arr.Count > 0 && AsInt(arr[^1]) is { } yi:
                    return yi;
                default:
                    continue;
            }
        }

        return null;
    }

    private static int? AsInt(JsonNode? n)
    {
        switch (n)
        {
            case JsonValue vv when vv.TryGetValue<long>(out var l):
                return (int)Math.Min(int.MaxValue, l);
            case JsonArray arr when arr.Count > 0 && AsInt(arr[^1]) is { } yi:
                return yi;
            case JsonObject oo when oo.TryGetPropertyValue("score", out var snode):
                return AsInt(snode);
            default:
                return null;
        }
    }

    private static long? ParseLongTry(JsonNode? n)
        => n switch
           {
               JsonValue v when v.TryGetValue<long>(out var lo) => lo,
               JsonValue sv when sv.ToString()?.Trim() is {} ts &&
                                  long.TryParse(ts, CultureInfo.InvariantCulture, out var lx) =>
                   lx,
               _ => null
           };

    private static string? IdToString(JsonNode? node)
        => node switch
        {
            JsonValue v when v.TryGetValue<long>(out var l) => l.ToString(CultureInfo.InvariantCulture),
            null => null,
            _ when long.TryParse(
                node.ToString().Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var id) =>
                id.ToString(CultureInfo.InvariantCulture),
            JsonValue xv => xv.ToString(),
            _ => node.ToString()
        };
}
