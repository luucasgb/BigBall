using System.Text.Json;
using BigBall.Shared.WorldCup;

namespace BigBall.Domain.Tests;

public class OpenFootballGroundToHostCityTests
{
    [Fact]
    public void Every_known_ground_in_fixture_json_resolves_to_host_id()
    {
        var path = FindWorldCup2026JsonPath();
        Assert.True(path is not null, "worldcup-2026.json not found; run test from solution tree.");

        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var grounds = new HashSet<string>(StringComparer.Ordinal);
        if (!doc.RootElement.TryGetProperty("matches", out var matches) || matches.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Invalid JSON: missing matches[]");
        }

        foreach (var m in matches.EnumerateArray())
        {
            if (m.TryGetProperty("ground", out var g) && g.ValueKind == JsonValueKind.String)
            {
                var s = g.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                {
                    grounds.Add(s.Trim());
                }
            }
        }

        foreach (var ground in grounds)
        {
            var id = OpenFootballGroundToHostCity.TryGetHostCityId(ground);
            Assert.True(id is not null, $"Missing map for OpenFootball ground: {ground}");
        }

        Assert.Equal(OpenFootballGroundToHostCity.KnownGrounds.Count, grounds.Count);
    }

    [Fact]
    public void MetLife_resolves_to_id_8()
    {
        Assert.Equal(8, OpenFootballGroundToHostCity.TryGetHostCityId("New York/New Jersey (East Rutherford)"));
    }

    private static string? FindWorldCup2026JsonPath()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null)
        {
            var p = Path.Combine(d.FullName, "src", "BigBall.Api", "Data", "worldcup-2026.json");
            if (File.Exists(p))
            {
                return p;
            }

            p = Path.Combine(d.FullName, "BigBall.Api", "Data", "worldcup-2026.json");
            if (File.Exists(p))
            {
                return p;
            }

            d = d.Parent;
        }

        return null;
    }
}
