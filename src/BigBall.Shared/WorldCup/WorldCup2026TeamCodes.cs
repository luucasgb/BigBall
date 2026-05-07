namespace BigBall.Shared.WorldCup;

/// <summary>Maps openfootball country names to short codes for display/flags (FIFA-style where applicable).</summary>
public static class WorldCup2026TeamCodes
{
    private static readonly IReadOnlyDictionary<string, string> CodeToName;

    private static readonly Dictionary<string, string> ByName = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Mexico"] = "MEX",
        ["South Africa"] = "RSA",
        ["South Korea"] = "KOR",
        ["Czech Republic"] = "CZE",
        ["Canada"] = "CAN",
        ["Bosnia & Herzegovina"] = "BIH",
        ["Qatar"] = "QAT",
        ["Switzerland"] = "SUI",
        ["Brazil"] = "BRA",
        ["Morocco"] = "MAR",
        ["Haiti"] = "HAI",
        ["Scotland"] = "SCO",
        ["USA"] = "USA",
        ["Paraguay"] = "PAR",
        ["Australia"] = "AUS",
        ["Turkey"] = "TUR",
        ["Germany"] = "GER",
        ["Curaçao"] = "CUW",
        ["Ivory Coast"] = "CIV",
        ["Ecuador"] = "ECU",
        ["Netherlands"] = "NED",
        ["Japan"] = "JPN",
        ["Sweden"] = "SWE",
        ["Tunisia"] = "TUN",
        ["Belgium"] = "BEL",
        ["Egypt"] = "EGY",
        ["Iran"] = "IRN",
        ["New Zealand"] = "NZL",
        ["Spain"] = "ESP",
        ["Cape Verde"] = "CPV",
        ["Saudi Arabia"] = "KSA",
        ["Uruguay"] = "URU",
        ["France"] = "FRA",
        ["Senegal"] = "SEN",
        ["Iraq"] = "IRQ",
        ["Norway"] = "NOR",
        ["Argentina"] = "ARG",
        ["Algeria"] = "ALG",
        ["Austria"] = "AUT",
        ["Jordan"] = "JOR",
        ["Portugal"] = "POR",
        ["DR Congo"] = "COD",
        ["Uzbekistan"] = "UZB",
        ["Colombia"] = "COL",
        ["England"] = "ENG",
        ["Croatia"] = "CRO",
        ["Ghana"] = "GHA",
        ["Panama"] = "PAN",
    };

    static WorldCup2026TeamCodes()
    {
        var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, code) in ByName)
        {
            d[code] = name;
        }

        CodeToName = d;
    }

    /// <summary>Short label for list UI (full country name or slot code).</summary>
    public static string ToDisplayName(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return "—";
        }

        return CodeToName.TryGetValue(code.Trim(), out var n) ? n : code;
    }

    /// <summary>Resolves a display code (max 32 chars). Bracket placeholders are passed through unchanged.</summary>
    public static string ToCode(string team)
    {
        if (string.IsNullOrWhiteSpace(team))
        {
            return "?";
        }

        var t = team.Trim();
        if (t.Length <= 32 && (char.IsDigit(t[0]) || t.Contains('/', StringComparison.Ordinal) || t.StartsWith('W') || t.StartsWith('L')))
        {
            return t.Length <= 32 ? t : t[..32];
        }

        if (ByName.TryGetValue(t, out var code))
        {
            return code;
        }

        // Fallback: compact initials (e.g. unknown future renames)
        var letters = string.Concat(t.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => char.ToUpperInvariant(s[0])));
        if (letters.Length is >= 3 and <= 8)
        {
            return letters[..Math.Min(letters.Length, 8)];
        }

        return letters.Length > 0 ? letters[..Math.Min(letters.Length, 8)] : "??";
    }
}
