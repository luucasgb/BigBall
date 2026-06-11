namespace BigBall.Web.Shared.UI.Primitives;

/// <summary>Portuguese display labels for match phases and group labels.</summary>
public static class MatchLabels
{
    /// <summary>Translates a raw group label (e.g. "Group A") to Portuguese ("Grupo A").</summary>
    public static string GroupLabel(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "";
        }

        var trimmed = raw.Trim();
        // OpenFootball stores group labels as "Group A", "Group B", ...
        if (trimmed.StartsWith("Group ", StringComparison.OrdinalIgnoreCase))
        {
            return "Grupo " + trimmed["Group ".Length..].Trim();
        }

        return trimmed;
    }

    /// <summary>Translates a MatchPhase enum name to a Portuguese phase label.</summary>
    public static string PhaseLabel(string? phase) => phase switch
    {
        "Groups" => "Fase de grupos",
        "RoundOf32" => "16-avos de final",
        "RoundOf16" => "Oitavas de final",
        "Quarters" => "Quartas de final",
        "Semis" => "Semifinal",
        "ThirdPlace" => "Disputa de 3º lugar",
        "Final" => "Final",
        _ => phase ?? ""
    };

    /// <summary>Display label preferring the (translated) group and falling back to the phase.</summary>
    public static string ForMatch(string? phase, string? groupLabel) =>
        string.IsNullOrWhiteSpace(groupLabel) ? PhaseLabel(phase) : GroupLabel(groupLabel);
}
