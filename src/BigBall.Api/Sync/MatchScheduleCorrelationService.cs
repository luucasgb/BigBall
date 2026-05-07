using BigBall.Domain.Entities;
using BigBall.Domain.SportsData;
using BigBall.Shared.WorldCup;
using Microsoft.Extensions.Caching.Memory;

namespace BigBall.Api.Sync;

/// <summary>Resolves persisted <see cref="Match"/> rows to vendor event ids via <see cref="ISportsDataSource.GetScheduleAsync"/> (one HTTP GET per day in warm cache).</summary>
public sealed class MatchScheduleCorrelationService(
    ISportsDataSource sportsData,
    IMemoryCache cache,
    ILogger<MatchScheduleCorrelationService> logger)
{
    private static readonly MemoryCacheEntryOptions CacheOptions =
        new() { SlidingExpiration = TimeSpan.FromMinutes(15) };

    internal async Task<bool> TryHydrateSportsApiIdAsync(Match entity, SportsMatchFetchTelemetry telemetry,
        CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(entity.ProviderExternalMatchId))
            return true;

        if (string.IsNullOrEmpty(entity.HomeCode) || string.IsNullOrEmpty(entity.AwayCode))
            return false;

        var kickoffUtc = entity.KickoffUtc;
        if (kickoffUtc.Kind != DateTimeKind.Utc)
            kickoffUtc = DateTime.SpecifyKind(kickoffUtc, DateTimeKind.Utc);

        var dayUtc = DateOnly.FromDateTime(kickoffUtc);
        var cacheKey = $"sap_sched_{dayUtc:O}_v1";

        if (!cache.TryGetValue(cacheKey, out IReadOnlyList<SportsScheduledFixture>? fixtures) || fixtures is null)
        {
            fixtures = await sportsData.GetScheduleAsync(dayUtc, telemetry, ct).ConfigureAwait(false);
            cache.Set(cacheKey, fixtures, CacheOptions);
        }

        var homePatterns = PatternsFor(entity.HomeCode);
        var awayPatterns = PatternsFor(entity.AwayCode);

        SportsScheduledFixture? best = null;
        var bestAbs = TimeSpan.MaxValue.TotalSeconds;

        foreach (var f in fixtures!)
        {
            if (!RoughlySameMoment(f.KickoffUtc, kickoffUtc, toleranceSeconds: 135))
                continue;

            if (!ProbablySameTeamLabel(f.HomeTeamLabel, homePatterns)
                || !ProbablySameTeamLabel(f.AwayTeamLabel, awayPatterns))
                continue;

            var abs = Math.Abs((f.KickoffUtc - kickoffUtc).TotalSeconds);
            if (best is null || abs < bestAbs)
            {
                best = f;
                bestAbs = abs;
            }
        }

        if (best is null)
        {
            logger.LogWarning("Correlation miss for Match {ExternalKey} KO {Kickoff}", entity.ExternalKey, kickoffUtc);
            return false;
        }

        entity.ProviderExternalMatchId = best.ExternalMatchId;
        logger.LogInformation("Correlation hit: ExternalKey {Ek} → provider id {Id}", entity.ExternalKey,
            best.ExternalMatchId);
        return true;
    }

    private static List<string> PatternsFor(string code)
    {
        var display = WorldCup2026TeamCodes.ToDisplayName(code.Trim());
        return new List<string> { code.Trim(), display.Trim() }.Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool ProbablySameTeamLabel(string? hay, IList<string> patterns)
    {
        if (string.IsNullOrWhiteSpace(hay))
            return false;

        foreach (var p in patterns)
        {
            if (string.IsNullOrEmpty(p))
                continue;

            if (hay.Contains(p, StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(hay.Trim(), p, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static bool RoughlySameMoment(DateTime fixtureKickoffUtc, DateTime oursKickoffUtc,
        double toleranceSeconds)
        => Math.Abs((oursKickoffUtc - fixtureKickoffUtc).TotalSeconds) <= toleranceSeconds;
}
