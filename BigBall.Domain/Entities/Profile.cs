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
}
