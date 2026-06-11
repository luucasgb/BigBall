using BigBall.Client.Core.Abstractions;

namespace BigBall.Web.Platform;

/// <summary>
/// Resolve o fuso horário escolhido pelo usuário (de <see cref="IUserProfileStore"/>) e converte
/// horários UTC para esse fuso. Componentes que exibem horários absolutos devem usar este provedor
/// em vez de <c>TimeZoneInfo.Local</c>, para que a escolha do usuário valha em todo o app.
/// </summary>
public interface IUserTimeZoneProvider
{
    TimeZoneInfo Current { get; }

    /// <summary>Rótulo amigável, ex.: <c>America/Sao_Paulo (UTC-03:00)</c>.</summary>
    string Label { get; }

    /// <summary>Rótulo compacto do deslocamento, ex.: <c>UTC-03:00</c> (para espaços apertados).</summary>
    string OffsetLabel { get; }

    DateTime ToLocal(DateTime utc);

    event Action? Changed;
}

public sealed class UserTimeZoneProvider : IUserTimeZoneProvider, IDisposable
{
    /// <summary>Fuso padrão quando o usuário ainda não escolheu um (app focado no Brasil).</summary>
    public const string DefaultTimeZoneId = "America/Sao_Paulo";

    private readonly IUserProfileStore _profileStore;
    private TimeZoneInfo _current;

    public UserTimeZoneProvider(IUserProfileStore profileStore)
    {
        _profileStore = profileStore;
        _current = Resolve(profileStore.Snapshot?.TimeZoneId);
        _profileStore.SnapshotChanged += OnSnapshotChanged;
    }

    public TimeZoneInfo Current => _current;

    public string Label => $"{_current.Id} ({FormatOffset(_current)})";

    public string OffsetLabel => FormatOffset(_current);

    public event Action? Changed;

    public DateTime ToLocal(DateTime utc)
    {
        var asUtc = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(asUtc, _current);
    }

    private void OnSnapshotChanged()
    {
        var next = Resolve(_profileStore.Snapshot?.TimeZoneId);
        if (string.Equals(next.Id, _current.Id, StringComparison.Ordinal))
        {
            return;
        }

        _current = next;
        Changed?.Invoke();
    }

    private static TimeZoneInfo Resolve(string? timeZoneId)
    {
        foreach (var candidate in new[] { timeZoneId, DefaultTimeZoneId })
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(candidate);
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                // tenta o próximo candidato
            }
        }

        return TimeZoneInfo.Local;
    }

    private static string FormatOffset(TimeZoneInfo tz)
    {
        var off = tz.GetUtcOffset(DateTime.UtcNow);
        var sign = off < TimeSpan.Zero ? "-" : "+";
        return $"UTC{sign}{Math.Abs(off.Hours):00}:{Math.Abs(off.Minutes):00}";
    }

    public void Dispose() => _profileStore.SnapshotChanged -= OnSnapshotChanged;
}
