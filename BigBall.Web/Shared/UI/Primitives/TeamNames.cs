using BigBall.Shared.WorldCup;

namespace BigBall.Web.Shared.UI.Primitives;

/// <summary>
/// Portuguese national-team display names, keyed by FIFA code. Display-only: the English
/// names from <see cref="WorldCup2026TeamCodes"/> stay intact for server-side FlashScore
/// correlation, so this lives in the web/UI layer rather than the shared domain utility.
/// </summary>
public static class TeamNames
{
    private static readonly Dictionary<string, string> PtByCode = new(StringComparer.OrdinalIgnoreCase)
    {
        ["MEX"] = "México",         ["RSA"] = "África do Sul",
        ["KOR"] = "Coreia do Sul",  ["CZE"] = "Tchéquia",
        ["CAN"] = "Canadá",         ["BIH"] = "Bósnia e Herzegovina",
        ["QAT"] = "Catar",          ["SUI"] = "Suíça",
        ["BRA"] = "Brasil",         ["MAR"] = "Marrocos",
        ["HAI"] = "Haiti",          ["SCO"] = "Escócia",
        ["USA"] = "Estados Unidos", ["PAR"] = "Paraguai",
        ["AUS"] = "Austrália",      ["TUR"] = "Turquia",
        ["GER"] = "Alemanha",       ["CUW"] = "Curaçao",
        ["CIV"] = "Costa do Marfim",["ECU"] = "Equador",
        ["NED"] = "Países Baixos",  ["JPN"] = "Japão",
        ["SWE"] = "Suécia",         ["TUN"] = "Tunísia",
        ["BEL"] = "Bélgica",        ["EGY"] = "Egito",
        ["IRN"] = "Irã",            ["NZL"] = "Nova Zelândia",
        ["ESP"] = "Espanha",        ["CPV"] = "Cabo Verde",
        ["KSA"] = "Arábia Saudita", ["URU"] = "Uruguai",
        ["FRA"] = "França",         ["SEN"] = "Senegal",
        ["IRQ"] = "Iraque",         ["NOR"] = "Noruega",
        ["ARG"] = "Argentina",      ["ALG"] = "Argélia",
        ["AUT"] = "Áustria",        ["JOR"] = "Jordânia",
        ["POR"] = "Portugal",       ["COD"] = "República Democrática do Congo",
        ["UZB"] = "Uzbequistão",    ["COL"] = "Colômbia",
        ["ENG"] = "Inglaterra",     ["CRO"] = "Croácia",
        ["GHA"] = "Gana",           ["PAN"] = "Panamá",
    };

    /// <summary>
    /// Portuguese name for a team code; falls back to the English display name (and then the
    /// raw code/placeholder such as "W97") for codes outside the map.
    /// </summary>
    public static string Display(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return "—";
        }

        var c = code.Trim();
        return PtByCode.TryGetValue(c, out var pt) ? pt : WorldCup2026TeamCodes.ToDisplayName(c);
    }
}
