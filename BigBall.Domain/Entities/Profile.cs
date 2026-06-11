namespace BigBall.Domain.Entities;

public sealed class Profile
{
    public required Guid Id { get; init; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsPlatformAdmin { get; set; }

    /// <summary>Instante em que o perfil de produto foi criado (alinhado a <c>auth.users.created_at</c> quando o trigger Supabase insere a linha).</summary>
    public DateTime CreateDate { get; set; }

    /// <summary>Fuso horário IANA escolhido pelo usuário (ex.: <c>America/Sao_Paulo</c>); usado para localizar os horários dos jogos. Nulo até o usuário definir.</summary>
    public string? TimeZoneId { get; set; }

    /// <summary>Verdadeiro quando o usuário excluiu sua conta (LGPD). A linha é mantida e anonimizada para preservar palpites e rankings dos bolões.</summary>
    public bool IsInactive { get; set; }

    /// <summary>Instante em que a conta foi desativada/anonimizada; nulo enquanto a conta está ativa.</summary>
    public DateTime? DeactivationDate { get; set; }
}
