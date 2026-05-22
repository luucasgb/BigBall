using System.Text.Json.Nodes;
using BigBall.Api.Data;
using BigBall.Api.Sync;
using BigBall.Domain.Entities;
using BigBall.Shared.Dtos;
using BigBall.Shared.WorldCup;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Integrations.FlashScore;

/// <summary>
/// One-shot backfill: search FlashScore for each WC2026 team display name and persist its badge URLs.
/// Idempotent — re-running skips teams that already have a non-null <see cref="Team.BadgeUrl"/>.
/// </summary>
public sealed class FlashScoreTeamSearchService(
    HttpClient http,
    IProviderDailyApiBudget budget,
    ILogger<FlashScoreTeamSearchService> logger)
{
    private const string SearchPath = "api/flashscore/v2/general/search";

    /// <summary>Excluded name fragments used to skip non-senior-men variants (Brazil U21, Brazil W, etc.).</summary>
    private static readonly string[] ExcludedNameFragments =
    {
        " U21", " U20", " U23", " U19", " U18", " U17", " Ol.", " W"
    };

    public async Task<TeamBackfillReportDto> SeedWorldCup2026TeamsAsync(
        BigBallDbContext db,
        CancellationToken ct)
    {
        var codes = WorldCup2026TeamCodes.AllCodes();
        var unresolved = new List<string>();
        var errors = new List<string>();
        var updated = 0;
        var skipped = 0;
        var httpCalls = 0;

        foreach (var code in codes)
        {
            if (ct.IsCancellationRequested)
                break;

            var displayName = WorldCup2026TeamCodes.ToDisplayName(code);
            var team = await db.Teams.FirstOrDefaultAsync(t => t.Code == code, ct).ConfigureAwait(false);

            if (team is not null && !string.IsNullOrEmpty(team.BadgeUrl))
            {
                skipped++;
                continue;
            }

            string? rawJson;
            try
            {
                var url = $"{SearchPath}?q={Uri.EscapeDataString(displayName)}";
                using var res = await http.GetAsync(url, ct).ConfigureAwait(false);
                httpCalls++;
                if (!res.IsSuccessStatusCode)
                {
                    errors.Add($"{code}: HTTP {(int)res.StatusCode}");
                    unresolved.Add(code);
                    continue;
                }
                rawJson = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                errors.Add($"{code}: {ex.GetType().Name} {ex.Message}");
                unresolved.Add(code);
                continue;
            }

            var pick = PickBestTeamMatch(rawJson, displayName);
            if (pick is null)
            {
                unresolved.Add(code);
                logger.LogInformation(
                    "FlashScore team search: no Soccer/Men team match for {Code} ({DisplayName}).",
                    code, displayName);
                continue;
            }

            if (team is null)
            {
                team = new Team { Code = code, DisplayName = displayName };
                db.Teams.Add(team);
            }
            else
            {
                team.DisplayName = displayName;
            }

            team.BadgeUrl = pick.ImageUrl;
            team.BadgeUrlSmall = pick.SmallImageUrl ?? team.BadgeUrlSmall;
            team.CountryImageUrl = pick.CountryImageUrl ?? team.CountryImageUrl;
            team.FlashScoreTeamId = pick.TeamId ?? team.FlashScoreTeamId;
            team.FlashScoreTeamUrl = pick.TeamUrl ?? team.FlashScoreTeamUrl;
            team.LastUpdatedUtc = DateTime.UtcNow;
            team.LastSource = "search";

            updated++;
        }

        if (httpCalls > 0)
        {
            await budget.AddConsumptionAsync(db, httpCalls, ct).ConfigureAwait(false);
        }

        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        return new TeamBackfillReportDto(
            TotalRequested: codes.Count,
            TotalUpdated: updated,
            TotalSkippedAlreadyCached: skipped,
            HttpCallsUsed: httpCalls,
            UnresolvedCodes: unresolved,
            Errors: errors);
    }

    /// <summary>
    /// Pick the best team result from a Search payload: Soccer/Men/non-age-group, prefer exact name match.
    /// Returns null when no candidate qualifies.
    /// </summary>
    internal static TeamSearchPick? PickBestTeamMatch(string searchJson, string displayName)
    {
        if (string.IsNullOrWhiteSpace(searchJson))
            return null;

        JsonNode? root;
        try { root = JsonNode.Parse(searchJson); }
        catch (System.Text.Json.JsonException) { return null; }

        if (root is not JsonArray arr || arr.Count == 0)
            return null;

        TeamSearchPick? exact = null;
        TeamSearchPick? caseInsensitive = null;
        TeamSearchPick? fallback = null;

        foreach (var item in arr)
        {
            if (item is null) continue;

            var type = item["type"]?.ToString();
            var sportId = AsInt(item["sport"]?["id"]);
            var gender = item["gender"]?.ToString();
            var name = item["name"]?.ToString();

            if (!string.Equals(type, "team", StringComparison.Ordinal)) continue;
            if (sportId != 1) continue;
            if (!string.Equals(gender, "Men", StringComparison.Ordinal)) continue;
            if (string.IsNullOrEmpty(name)) continue;
            if (NameLooksExcluded(name)) continue;

            var pick = new TeamSearchPick(
                Name: name,
                ImageUrl: NonEmpty(item["image_path"]?.ToString()),
                SmallImageUrl: NonEmpty(item["small_image_path"]?.ToString()),
                CountryImageUrl: NonEmpty(item["country_image_path"]?.ToString()),
                TeamId: NonEmpty(item["id"]?.ToString()),
                TeamUrl: NonEmpty(item["url"]?.ToString()));

            if (pick.ImageUrl is null) continue;

            if (string.Equals(name, displayName, StringComparison.Ordinal))
            {
                exact ??= pick;
            }
            else if (string.Equals(name, displayName, StringComparison.OrdinalIgnoreCase))
            {
                caseInsensitive ??= pick;
            }
            else
            {
                fallback ??= pick;
            }
        }

        return exact ?? caseInsensitive ?? fallback;
    }

    private static bool NameLooksExcluded(string name)
    {
        foreach (var frag in ExcludedNameFragments)
        {
            if (name.Contains(frag, StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    private static int? AsInt(JsonNode? n) => n switch
    {
        JsonValue v when v.TryGetValue<long>(out var l) => (int)l,
        JsonValue sv when int.TryParse(sv.ToString(), out var x) => x,
        _ => null,
    };

    private static string? NonEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;

    internal sealed record TeamSearchPick(
        string Name,
        string? ImageUrl,
        string? SmallImageUrl,
        string? CountryImageUrl,
        string? TeamId,
        string? TeamUrl);
}
