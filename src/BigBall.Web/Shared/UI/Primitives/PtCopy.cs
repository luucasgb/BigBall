namespace BigBall.Web.Shared.UI.Primitives;

/// <summary>Portuguese UI copy for numeric plurals (avoids bolão(ões) / ativo(s) style).</summary>
public static class PtCopy
{
    public static string ActivePoolsLine(int count) => count switch
    {
        0 => "Nenhum bolão ativo",
        1 => "1 bolão ativo",
        _ => $"{count} bolões ativos"
    };

    public static string MemberPoolsLine(int count) => count switch
    {
        0 => "Membro · 0 bolões",
        1 => "Membro · 1 bolão",
        _ => $"Membro · {count} bolões"
    };

    /// <summary>KPI subtitle when the user has at least one pool: "em 1 bolão" / "em N bolões".</summary>
    public static string InPoolsFooter(int count) => count switch
    {
        1 => "em 1 bolão",
        _ => $"em {count} bolões"
    };

    public static string SavePalpiteEmBoloes(int count) => count switch
    {
        0 => "Salvar palpite",
        1 => "Salvar palpite em 1 bolão",
        _ => $"Salvar palpite em {count} bolões"
    };
}
