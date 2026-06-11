namespace BigBall.Shared.Dtos;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string Token, ProfileDto Profile);
public sealed record OAuthRedirectUrlResponse(string Url);

public sealed record RegisterRequest(
    string Email,
    string Password,
    string? DisplayName = null,
    string? EmailRedirectTo = null);

/// <summary>
/// <see cref="Session"/> is set when Supabase returns a session (e.g. e-mail auto-confirm).
/// Otherwise <see cref="RequiresEmailConfirmation"/> is true after successful signup.
/// </summary>
public sealed record RegisterResponse(
    LoginResponse? Session,
    bool RequiresEmailConfirmation,
    string? PendingEmail = null);

public sealed record ProfileDto(Guid Id, string Email, string DisplayName, string? AvatarUrl, DateTime CreateDate, string? TimeZoneId = null);

/// <summary>Atualização parcial do perfil: campos nulos são ignorados pelo servidor.</summary>
public sealed record UpdateProfileRequest(string? DisplayName = null, string? TimeZoneId = null);

/// <summary>Distribuição de palpites do usuário por faixa de pontuação (tier do <c>ScoringEngine</c>).</summary>
public sealed record ScoringBandDto(int Tier, int Count);

/// <summary>Uma linha do histórico recente de palpites do usuário. Campos crus; a formatação ocorre no cliente.</summary>
public sealed record ProfileActivityRowDto(
    string HomeCode,
    string AwayCode,
    string PoolName,
    int? ResultHome,
    int? ResultAway,
    int PredHome,
    int PredAway,
    int Points,
    DateTime KickoffUtc);

/// <summary>Estatísticas globais do perfil, agregadas sobre todos os palpites do usuário em todas as pools.</summary>
public sealed record ProfileStatsDto(
    int SubmittedCount,
    int EligibleCount,
    IReadOnlyList<ScoringBandDto> Bands,
    IReadOnlyList<ProfileActivityRowDto> RecentActivity);
