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

public sealed record ProfileDto(Guid Id, string Email, string DisplayName, string? AvatarUrl, DateTime CreateDate);
