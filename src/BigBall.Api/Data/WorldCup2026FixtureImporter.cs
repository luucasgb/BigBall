using System.Text.Json;
using BigBall.Domain.Entities;
using BigBall.Domain.Enums;
using BigBall.Shared.WorldCup;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Data;

public static class WorldCup2026FixtureImporter
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task ImportAsync(BigBallDbContext db, string jsonFilePath, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(jsonFilePath);
        var root = await JsonSerializer.DeserializeAsync<WorldCupJsonRoot>(stream, JsonOptions, ct).ConfigureAwait(false);
        if (root?.matches is null)
        {
            return;
        }

        var venueById = await db.HostCities
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.VenueName, ct)
            .ConfigureAwait(false);

        foreach (var m in root.matches)
        {
            if (m is not { date: { } d, time: { } t, team1: { } t1, team2: { } t2, round: { } r })
            {
                continue;
            }

            if (!OpenFootballScheduleParse.TryParseKickoffUtc(d, t, out var kickoffUtc))
            {
                continue;
            }

            var phaseStr = OpenFootballScheduleParse.MapRoundToPhaseString(r);
            if (!Enum.TryParse<MatchPhase>(phaseStr, out var phase))
            {
                phase = MatchPhase.Groups;
            }

            var ext = OpenFootballScheduleParse.BuildExternalKey(m.num, d, t, t1, t2, r);
            var home = WorldCup2026TeamCodes.ToCode(t1);
            var away = WorldCup2026TeamCodes.ToCode(t2);

            var groupLabel = string.IsNullOrWhiteSpace(m.group) ? null : m.group.Trim();
            if (groupLabel is { Length: > 64 })
            {
                groupLabel = groupLabel[..64];
            }

            var ground = string.IsNullOrWhiteSpace(m.ground) ? null : m.ground.Trim();
            if (ground is { Length: > 200 })
            {
                ground = ground[..200];
            }

            int? hostCityId = OpenFootballGroundToHostCity.TryGetHostCityId(ground);
            var venue = ground;
            if (hostCityId is { } hid && venueById.TryGetValue(hid, out var stadiumName))
            {
                venue = stadiumName;
            }
            if (venue is { Length: > 200 })
            {
                venue = venue[..200];
            }

            var existing = await db.Matches.FirstOrDefaultAsync(x => x.ExternalKey == ext, ct).ConfigureAwait(false);
            if (existing is not null)
            {
                existing.Phase = phase;
                existing.GroupLabel = groupLabel;
                existing.HomeCode = home;
                existing.AwayCode = away;
                existing.KickoffUtc = kickoffUtc;
                existing.Venue = venue;
                existing.HostCityId = hostCityId;
                existing.Status = MatchStatus.Scheduled;
            }
            else
            {
                db.Matches.Add(new Match
                {
                    Id = Guid.NewGuid(),
                    Phase = phase,
                    GroupLabel = groupLabel,
                    HomeCode = home,
                    AwayCode = away,
                    ExternalKey = ext,
                    KickoffUtc = kickoffUtc,
                    Venue = venue,
                    HostCityId = hostCityId,
                    Status = MatchStatus.Scheduled,
                });
            }
        }

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private sealed class WorldCupJsonRoot
    {
        public List<WorldCupJsonMatch>? matches { get; set; }
    }

    private sealed class WorldCupJsonMatch
    {
        public string? round { get; set; }
        public string? date { get; set; }
        public string? time { get; set; }
        public string? team1 { get; set; }
        public string? team2 { get; set; }
        public string? group { get; set; }
        public string? ground { get; set; }
        public int? num { get; set; }
    }
}
