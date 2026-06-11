namespace BigBall.Web.Shared.UI.Primitives;

/// <summary>
/// Portuguese date-part names. Blazor WebAssembly runs with invariant globalization,
/// so <c>CultureInfo("pt-BR")</c> falls back to English month/day names. These explicit
/// maps guarantee Portuguese output regardless of the runtime culture.
/// </summary>
public static class PtDates
{
    private static readonly string[] AbbreviatedDays =
        { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb" };

    private static readonly string[] FullDays =
        { "domingo", "segunda-feira", "terça-feira", "quarta-feira", "quinta-feira", "sexta-feira", "sábado" };

    private static readonly string[] AbbreviatedMonths =
        { "jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez" };

    private static readonly string[] FullMonths =
        { "janeiro", "fevereiro", "março", "abril", "maio", "junho", "julho", "agosto", "setembro", "outubro", "novembro", "dezembro" };

    /// <summary>Abbreviated, capitalised day name (e.g. "Qui").</summary>
    public static string AbbreviatedDay(DayOfWeek day) => AbbreviatedDays[(int)day];

    /// <summary>Full, lowercase day name (e.g. "quinta-feira").</summary>
    public static string FullDay(DayOfWeek day) => FullDays[(int)day];

    /// <summary>Abbreviated, lowercase month name (e.g. "jun"). <paramref name="month"/> is 1-12.</summary>
    public static string AbbreviatedMonth(int month) => AbbreviatedMonths[month - 1];

    /// <summary>Full, lowercase month name (e.g. "junho"). <paramref name="month"/> is 1-12.</summary>
    public static string FullMonth(int month) => FullMonths[month - 1];
}
