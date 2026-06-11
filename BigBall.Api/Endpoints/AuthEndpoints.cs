using System.Security.Claims;
using System.Text.Json;
using BigBall.Api.Auth;
using BigBall.Api.Data;
using BigBall.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BigBall.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest req, SupabasePasswordAuthClient supabase, BigBallDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            {
                return Results.BadRequest(new { error = "Email e senha são obrigatórios." });
            }

            var auth = await supabase.SignInAsync(req.Email.Trim(), req.Password, ct);
            if (auth is null)
            {
                return Results.Unauthorized();
            }

            var existingProfile = await db.Profiles.FirstOrDefaultAsync(p => p.Id == auth.User.Id, ct);
            if (existingProfile is not null && existingProfile.IsInactive)
            {
                return Results.Json(new { error = "Esta conta foi excluída." }, statusCode: StatusCodes.Status403Forbidden);
            }

            var profile = await UpsertProfileAsync(db, auth.User, ct);
            return Results.Ok(new LoginResponse(
                auth.AccessToken,
                new ProfileDto(profile.Id, profile.Email, profile.DisplayName, profile.AvatarUrl, profile.CreateDate, profile.TimeZoneId)));
        })
        .AllowAnonymous()
        .WithName("Login");

        group.MapPost("/register", async (RegisterRequest req, SupabasePasswordAuthClient supabase, BigBallDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            {
                return Results.BadRequest(new { error = "Email e senha são obrigatórios." });
            }

            var displayName = string.IsNullOrWhiteSpace(req.DisplayName) ? null : req.DisplayName.Trim();
            var redirect = string.IsNullOrWhiteSpace(req.EmailRedirectTo) ? null : req.EmailRedirectTo.Trim();

            var result = await supabase.SignUpAsync(req.Email.Trim(), req.Password, displayName, redirect, ct);
            if (result.ErrorMessage is not null)
            {
                return Results.BadRequest(new { error = result.ErrorMessage });
            }

            if (result.Session is not null)
            {
                var profile = await UpsertProfileAsync(db, result.Session.User, ct);
                var login = new LoginResponse(
                    result.Session.AccessToken,
                    new ProfileDto(profile.Id, profile.Email, profile.DisplayName, profile.AvatarUrl, profile.CreateDate, profile.TimeZoneId));
                return Results.Ok(new RegisterResponse(login, RequiresEmailConfirmation: false));
            }

            if (result.UserPendingConfirmation is not null)
            {
                var email = result.UserPendingConfirmation.Email;
                return Results.Ok(new RegisterResponse(
                    Session: null,
                    RequiresEmailConfirmation: true,
                    PendingEmail: string.IsNullOrWhiteSpace(email) ? req.Email.Trim() : email));
            }

            return Results.BadRequest(new { error = "Não foi possível concluir o cadastro." });
        })
        .AllowAnonymous()
        .WithName("Register");

        group.MapGet("/google-url", (string redirectTo, IOptions<SupabaseAuthOptions> options) =>
        {
            if (string.IsNullOrWhiteSpace(redirectTo))
            {
                return Results.BadRequest(new { error = "redirectTo é obrigatório." });
            }

            var cfg = options.Value;
            var encodedRedirect = Uri.EscapeDataString(redirectTo);
            var url = $"{cfg.AuthIssuer}/authorize?provider=google&redirect_to={encodedRedirect}";
            return Results.Ok(new OAuthRedirectUrlResponse(url));
        })
        .AllowAnonymous()
        .WithName("GetGoogleOAuthUrl");

        group.MapGet("/me", async (ClaimsPrincipal user, BigBallDbContext db, CancellationToken ct) =>
        {
            var userId = user.RequireUserId();
            var email = ResolveJwtEmail(user) ?? string.Empty;
            var displayFromJwt = ResolveJwtDisplayName(user);
            var avatarFromJwt = ResolveJwtAvatarUrl(user);

            var profile = await db.Profiles.FirstOrDefaultAsync(p => p.Id == userId, ct);
            if (profile is not null && profile.IsInactive)
            {
                // Conta excluída (LGPD): não repopular dados anonimizados a partir do JWT residual.
                return Results.Json(new { error = "Esta conta foi excluída." }, statusCode: StatusCodes.Status403Forbidden);
            }

            if (profile is null)
            {
                profile = new BigBall.Domain.Entities.Profile
                {
                    Id = userId,
                    Email = email,
                    DisplayName = displayFromJwt ?? DeriveDisplayName(email),
                    CreateDate = DateTime.UtcNow
                };
                if (!string.IsNullOrWhiteSpace(avatarFromJwt))
                {
                    profile.AvatarUrl = avatarFromJwt;
                }

                db.Profiles.Add(profile);
                await db.SaveChangesAsync(ct);
            }
            else
            {
                var changed = false;
                if (!string.IsNullOrWhiteSpace(email) && !string.Equals(profile.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    profile.Email = email;
                    changed = true;
                }

                // DisplayName não é mais sincronizado do JWT após a criação do perfil:
                // o usuário pode editá-lo no app (PUT /me) e essa escolha deve prevalecer.

                if (!string.IsNullOrWhiteSpace(avatarFromJwt) &&
                    !string.Equals(profile.AvatarUrl, avatarFromJwt, StringComparison.Ordinal))
                {
                    profile.AvatarUrl = avatarFromJwt;
                    changed = true;
                }

                if (changed)
                {
                    await db.SaveChangesAsync(ct);
                }
            }

            return Results.Ok(new ProfileDto(profile.Id, profile.Email, profile.DisplayName, profile.AvatarUrl, profile.CreateDate, profile.TimeZoneId));
        })
        .RequireAuthorization()
        .WithName("GetMyProfile");

        group.MapDelete("/me", async (ClaimsPrincipal user, SupabasePasswordAuthClient supabase, BigBallDbContext db, CancellationToken ct) =>
        {
            var userId = user.RequireUserId();

            var profile = await db.Profiles.FirstOrDefaultAsync(p => p.Id == userId, ct);
            if (profile is not null && !profile.IsInactive)
            {
                // Soft-delete + anonimização (LGPD). A linha é mantida para preservar palpites e rankings dos bolões.
                profile.Email = $"deleted-{userId:N}@deleted.bigball";
                profile.DisplayName = "Usuário excluído";
                profile.AvatarUrl = null;
                profile.CreateDate = DateTime.UnixEpoch;
                profile.IsInactive = true;
                profile.DeactivationDate = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
            }

            // Exclusão real no provedor de identidade (best-effort): impede login futuro e remove o e-mail do auth.users.
            await supabase.DeleteAuthUserAsync(userId, ct);

            return Results.Ok();
        })
        .RequireAuthorization()
        .WithName("DeleteMyAccount");

        group.MapPut("/me", async (UpdateProfileRequest req, ClaimsPrincipal user, BigBallDbContext db, CancellationToken ct) =>
        {
            var userId = user.RequireUserId();
            var profile = await db.Profiles.FirstOrDefaultAsync(p => p.Id == userId, ct);
            if (profile is null || profile.IsInactive)
            {
                return Results.NotFound(new { error = "Perfil não encontrado." });
            }

            var changed = false;

            if (req.DisplayName is not null)
            {
                var name = req.DisplayName.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    return Results.BadRequest(new { error = "O nome de exibição não pode ficar vazio." });
                }
                if (name.Length > 120)
                {
                    return Results.BadRequest(new { error = "O nome de exibição deve ter no máximo 120 caracteres." });
                }
                if (!string.Equals(profile.DisplayName, name, StringComparison.Ordinal))
                {
                    profile.DisplayName = name;
                    changed = true;
                }
            }

            if (req.TimeZoneId is not null)
            {
                var tz = req.TimeZoneId.Trim();
                if (tz.Length == 0)
                {
                    return Results.BadRequest(new { error = "Fuso horário inválido." });
                }
                try
                {
                    _ = TimeZoneInfo.FindSystemTimeZoneById(tz);
                }
                catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
                {
                    return Results.BadRequest(new { error = $"Fuso horário desconhecido: {tz}." });
                }
                if (!string.Equals(profile.TimeZoneId, tz, StringComparison.Ordinal))
                {
                    profile.TimeZoneId = tz;
                    changed = true;
                }
            }

            if (changed)
            {
                await db.SaveChangesAsync(ct);
            }

            return Results.Ok(new ProfileDto(profile.Id, profile.Email, profile.DisplayName, profile.AvatarUrl, profile.CreateDate, profile.TimeZoneId));
        })
        .RequireAuthorization()
        .WithName("UpdateMyProfile");

        group.MapGet("/me/stats", async (ClaimsPrincipal user, ProfileStatsService stats, CancellationToken ct) =>
        {
            var userId = user.RequireUserId();
            return Results.Ok(await stats.BuildAsync(userId, ct));
        })
        .RequireAuthorization()
        .WithName("GetMyProfileStats");

        return app;
    }

    private static async Task<BigBall.Domain.Entities.Profile> UpsertProfileAsync(
        BigBallDbContext db,
        SupabaseAuthUser user,
        CancellationToken ct)
    {
        var existing = await db.Profiles.FirstOrDefaultAsync(p => p.Id == user.Id, ct);
        var displayName = user.UserMetadata?.FullName
                          ?? user.UserMetadata?.Name
                          ?? DeriveDisplayName(user.Email);
        if (existing is not null)
        {
            existing.DisplayName = displayName;
            existing.Email = user.Email;
            await db.SaveChangesAsync(ct);
            return existing;
        }

        var profile = new BigBall.Domain.Entities.Profile
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = displayName,
            CreateDate = DateTime.UtcNow
        };
        db.Profiles.Add(profile);
        await db.SaveChangesAsync(ct);
        return profile;
    }

    private static string DeriveDisplayName(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "Usuário BigBall";
        }
        var at = email.IndexOf('@');
        var local = at > 0 ? email[..at] : email;
        var parts = local.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
    }

    /// <summary>
    /// Supabase puts OAuth profile fields in a JSON claim <c>user_metadata</c>; email may also appear only there.
    /// ASP.NET default inbound claim map renames <c>email</c> to ClaimTypes.Email unless <see cref="Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions.MapInboundClaims"/> is false.
    /// </summary>
    private static string? ResolveJwtEmail(ClaimsPrincipal user)
    {
        var direct = user.FindFirst("email")?.Value
                     ?? user.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrWhiteSpace(direct))
        {
            return direct;
        }

        return TryGetUserMetadataString(user, "email");
    }

    private static string? ResolveJwtDisplayName(ClaimsPrincipal user)
    {
        var fromMeta = TryGetUserMetadataString(user, "full_name")
                       ?? TryGetUserMetadataString(user, "name");
        if (!string.IsNullOrWhiteSpace(fromMeta))
        {
            return fromMeta;
        }

        return user.FindFirst("name")?.Value;
    }

    private static string? ResolveJwtAvatarUrl(ClaimsPrincipal user) =>
        TryGetUserMetadataString(user, "avatar_url")
        ?? TryGetUserMetadataString(user, "picture");

    private static string? TryGetUserMetadataString(ClaimsPrincipal user, string propertyName)
    {
        var raw = user.FindFirst("user_metadata")?.Value;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (!doc.RootElement.TryGetProperty(propertyName, out var prop))
            {
                return null;
            }

            return prop.ValueKind switch
            {
                JsonValueKind.String => prop.GetString(),
                _ => null
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
