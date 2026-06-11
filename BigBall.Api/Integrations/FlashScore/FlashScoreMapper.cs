using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using BigBall.Api.Configuration;
using BigBall.Domain.SportsData;

namespace BigBall.Api.Integrations.FlashScore;

/// <summary>Maps FlashScore (RapidAPI) JSON payloads to canonical <see cref="SportsMatchSnapshot"/> (TechSpec §6.2.2).</summary>
public static class FlashScoreMapper
{
    public const string ProviderName = SportsDataProviderNames.FlashScore;

    /// <summary>Maps <c>/matches/details</c> (and optional <c>/matches/penalties</c>) into the canonical snapshot.</summary>
    public static SportsMatchSnapshot Map(
        string matchDetailsJson,
        string? matchPenaltiesJson = null)
    {
        var root = JsonNode.Parse(matchDetailsJson)
                     ?? throw new JsonException("Empty match details payload.");

        var externalId = root["match_id"]?.ToString()
                         ?? throw new JsonException("Missing match_id.");

        var kickoffUtc = ResolveKickoffUtc(root);

        var status = root["match_status"] as JsonObject;
        var scores = root["scores"] as JsonObject;

        var phase = MapLifecycle(status, scores);

        var (regsH, regsA, trReliable) = ExtractRegularTimeGoals(scores, phase);

        var wentEt = LifecycleImpliesExtraTime(phase)
                     || AsBool(status?["is_finished_after_extra_time"]) == true
                     || scores?["home_extra_time"] is JsonValue
                     || scores?["away_extra_time"] is JsonValue;

        var wentPens = LifecycleImpliesPenalties(phase)
                       || AsBool(status?["is_finished_after_penalties"]) == true
                       || scores?["home_penalties"] is JsonValue
                       || scores?["away_penalties"] is JsonValue;

        var winner = wentPens ? ResolvePenaltyWinner(status, scores, matchPenaltiesJson) : null;

        var origin = ResolveResultOrigin(trReliable, regsH, regsA, phase);

        var home = root["home_team"] as JsonObject;
        var away = root["away_team"] as JsonObject;

        return new SportsMatchSnapshot(
            ProviderName,
            externalId,
            kickoffUtc,
            phase,
            ProviderStatusCode: null,
            regsH,
            regsA,
            trReliable,
            wentEt,
            wentPens,
            winner,
            origin)
        {
            HomeTeamImageUrl = AsString(home?["image_path"]),
            HomeTeamImageUrlSmall = AsString(home?["small_image_path"]),
            HomeTeamFlashScoreId = AsString(home?["team_id"]),
            HomeTeamFlashScoreUrl = AsString(home?["team_url"]),
            AwayTeamImageUrl = AsString(away?["image_path"]),
            AwayTeamImageUrlSmall = AsString(away?["small_image_path"]),
            AwayTeamFlashScoreId = AsString(away?["team_id"]),
            AwayTeamFlashScoreUrl = AsString(away?["team_url"]),
        };
    }

    /// <summary>Derives canonical phase from FlashScore boolean flags + score-shape signals.</summary>
    public static MatchLifecyclePhase MapLifecycle(JsonObject? status, JsonObject? scores)
    {
        if (status is null)
            return MatchLifecyclePhase.Unknown;

        if (AsBool(status["is_cancelled"]) == true)
            return MatchLifecyclePhase.Canceled;
        if (AsBool(status["is_postponed"]) == true)
            return MatchLifecyclePhase.Postponed;

        if (AsBool(status["is_finished_after_penalties"]) == true)
            return MatchLifecyclePhase.FinishedAfterPenalties;
        if (AsBool(status["is_finished_after_extra_time"]) == true)
            return MatchLifecyclePhase.FinishedAfterExtraTime;
        if (AsBool(status["is_finished"]) == true)
            return MatchLifecyclePhase.FinishedRegulation;

        var inProgress = AsBool(status["is_in_progress"]) == true;
        var started = AsBool(status["is_started"]) == true;

        if (inProgress)
        {
            var hasPens = scores?["home_penalties"] is JsonValue || scores?["away_penalties"] is JsonValue;
            if (hasPens)
                return MatchLifecyclePhase.PenaltyShootoutInProgress;

            var hasEt = scores?["home_extra_time"] is JsonValue || scores?["away_extra_time"] is JsonValue;
            if (hasEt)
                return MatchLifecyclePhase.ExtraTimeFirstHalf;

            var p2H = AsInt(scores?["home_2nd_half"]);
            var p2A = AsInt(scores?["away_2nd_half"]);
            if ((p2H is not null && p2H.Value > 0) || (p2A is not null && p2A.Value > 0))
                return MatchLifecyclePhase.SecondHalf;

            return MatchLifecyclePhase.FirstHalf;
        }

        // Started but not in progress and not finished — most likely halftime intermission.
        if (started)
            return MatchLifecyclePhase.Halftime;

        return MatchLifecyclePhase.NotStarted;
    }

    /// <summary>
    /// Extracts regular-time (90 min + stoppage) goals from FlashScore scores. Prefers <c>home_total</c>/<c>away_total</c>
    /// and falls back to summing <c>home_1st_half + home_2nd_half</c>. Marks unreliable for in-flight live phases.
    /// </summary>
    public static (int? home, int? away, bool reliable) ExtractRegularTimeGoals(
        JsonObject? scores,
        MatchLifecyclePhase phase)
    {
        if (scores is null)
            return (null, null, false);

        var totalH = AsInt(scores["home_total"]);
        var totalA = AsInt(scores["away_total"]);

        var p1H = AsInt(scores["home_1st_half"]);
        var p2H = AsInt(scores["home_2nd_half"]);
        var p1A = AsInt(scores["away_1st_half"]);
        var p2A = AsInt(scores["away_2nd_half"]);

        int? regsH = totalH;
        int? regsA = totalA;

        if (regsH is null && p1H is not null && p2H is not null)
            regsH = p1H + p2H;
        if (regsA is null && p1A is not null && p2A is not null)
            regsA = p1A + p2A;

        var hasBoth = regsH is not null && regsA is not null;

        var reliable = hasBoth && phase switch
        {
            MatchLifecyclePhase.FinishedRegulation => true,
            MatchLifecyclePhase.FinishedAfterExtraTime => true,
            MatchLifecyclePhase.FinishedAfterPenalties => true,
            MatchLifecyclePhase.SecondHalf => true,
            MatchLifecyclePhase.Halftime => true,
            MatchLifecyclePhase.ExtraTimeFirstHalf => true,
            MatchLifecyclePhase.ExtraTimeSecondHalf => true,
            MatchLifecyclePhase.PenaltyShootoutInProgress => true,
            _ => false,
        };

        return (regsH, regsA, reliable);
    }

    /// <summary>Reads <c>match_status.final_winner</c>; returns null if absent or if no shootout occurred.</summary>
    public static bool? ExtractPenaltyWinnerIsHome(JsonObject? status)
    {
        if (status is null)
            return null;
        if (AsBool(status["is_finished_after_penalties"]) != true)
            return null;

        var fw = status["final_winner"]?.ToString()?.Trim();
        return fw switch
        {
            "home" => true,
            "away" => false,
            _ => null,
        };
    }

    /// <summary>Only request the penalties endpoint when the match payload already signals a shootout.</summary>
    public static bool ShouldRequestPenaltiesEndpoint(string matchDetailsJson)
    {
        if (string.IsNullOrWhiteSpace(matchDetailsJson))
            return false;

        JsonNode? root;
        try
        {
            root = JsonNode.Parse(matchDetailsJson);
        }
        catch (JsonException)
        {
            return false;
        }

        if (root is null)
            return false;

        var status = root["match_status"] as JsonObject;
        if (AsBool(status?["is_finished_after_penalties"]) == true)
            return true;

        var scores = root["scores"] as JsonObject;
        return scores?["home_penalties"] is JsonValue
            || scores?["away_penalties"] is JsonValue;
    }

    private static bool? ResolvePenaltyWinner(JsonObject? status, JsonObject? scores, string? penaltiesJson)
    {
        var fromStatus = ExtractPenaltyWinnerIsHome(status);
        if (fromStatus is not null)
            return fromStatus;

        var hPen = AsInt(scores?["home_penalties"]);
        var aPen = AsInt(scores?["away_penalties"]);
        if (hPen is { } hv && aPen is { } av && hv != av)
            return hv > av;

        if (string.IsNullOrWhiteSpace(penaltiesJson))
            return null;

        JsonNode? doc;
        try { doc = JsonNode.Parse(penaltiesJson); }
        catch (JsonException) { return null; }

        if (doc is null)
            return null;

        var dhPen = AsInt(doc["home_score"] ?? doc["homeScore"] ?? doc["home"]);
        var daPen = AsInt(doc["away_score"] ?? doc["awayScore"] ?? doc["away"]);
        if (dhPen is { } dh && daPen is { } da && dh != da)
            return dh > da;

        return null;
    }

    private static SportsResultOrigin ResolveResultOrigin(
        bool trReliable,
        int? regsH,
        int? regsA,
        MatchLifecyclePhase phase)
    {
        if (trReliable && regsH is not null && regsA is not null)
            return SportsResultOrigin.ProviderComplete;

        var lifecycleNeedsTr = phase is MatchLifecyclePhase.FinishedRegulation
            or MatchLifecyclePhase.FinishedAfterExtraTime
            or MatchLifecyclePhase.FinishedAfterPenalties;

        if (lifecycleNeedsTr && (regsH is null || regsA is null))
            return SportsResultOrigin.GapRegularTimeUnresolved;

        if (regsH is null && regsA is null)
            return SportsResultOrigin.Unknown;

        return SportsResultOrigin.ProviderPartial;
    }

    private static DateTime ResolveKickoffUtc(JsonNode root)
    {
        var sec = AsLong(root["timestamp"])
                  ?? throw new JsonException("Missing timestamp.");
        return DateTimeOffset.FromUnixTimeSeconds(sec).UtcDateTime;
    }

    private static bool LifecycleImpliesExtraTime(MatchLifecyclePhase phase) =>
        phase is MatchLifecyclePhase.ExtraTimeFirstHalf
               or MatchLifecyclePhase.ExtraTimeSecondHalf
               or MatchLifecyclePhase.FinishedAfterExtraTime
               or MatchLifecyclePhase.PenaltyShootoutInProgress
               or MatchLifecyclePhase.FinishedAfterPenalties;

    private static bool LifecycleImpliesPenalties(MatchLifecyclePhase phase) =>
        phase is MatchLifecyclePhase.PenaltyShootoutInProgress
               or MatchLifecyclePhase.FinishedAfterPenalties;

    private static bool? AsBool(JsonNode? n) => n switch
    {
        JsonValue v when v.TryGetValue<bool>(out var b) => b,
        JsonValue sv when bool.TryParse(sv.ToString(), out var b) => b,
        _ => null,
    };

    private static int? AsInt(JsonNode? n) => n switch
    {
        JsonValue v when v.TryGetValue<long>(out var l) => (int)Math.Min(int.MaxValue, l),
        JsonValue sv when int.TryParse(sv.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var x) => x,
        _ => null,
    };

    private static long? AsLong(JsonNode? n) => n switch
    {
        JsonValue v when v.TryGetValue<long>(out var l) => l,
        JsonValue sv when long.TryParse(sv.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var lx) => lx,
        _ => null,
    };

    private static string? AsString(JsonNode? n)
    {
        if (n is null) return null;
        var s = n.ToString();
        return string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
