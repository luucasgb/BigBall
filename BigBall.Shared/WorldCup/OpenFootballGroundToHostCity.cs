namespace BigBall.Shared.WorldCup;

/// <summary>Maps OpenFootball <c>ground</c> strings from the WC2026 schedule to host_cities <c>id</c> (1–16).</summary>
public static class OpenFootballGroundToHostCity
{
    private static readonly IReadOnlyDictionary<string, int> ByGround = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["Atlanta"] = 1,
        ["Boston (Foxborough)"] = 2,
        ["Dallas (Arlington)"] = 3,
        ["Houston"] = 4,
        ["Kansas City"] = 5,
        ["Los Angeles (Inglewood)"] = 6,
        ["Miami (Miami Gardens)"] = 7,
        ["New York/New Jersey (East Rutherford)"] = 8,
        ["Philadelphia"] = 9,
        ["San Francisco Bay Area (Santa Clara)"] = 10,
        ["Seattle"] = 11,
        ["Toronto"] = 12,
        ["Vancouver"] = 13,
        ["Guadalajara (Zapopan)"] = 14,
        ["Mexico City"] = 15,
        ["Monterrey (Guadalupe)"] = 16,
    };

    public static IReadOnlyCollection<string> KnownGrounds => ByGround.Keys.ToList();

    public static int? TryGetHostCityId(string? ground) =>
        string.IsNullOrWhiteSpace(ground) ? null
        : ByGround.TryGetValue(ground.Trim(), out var id) ? id : null;
}
