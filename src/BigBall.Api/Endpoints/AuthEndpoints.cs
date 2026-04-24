using BigBall.Api.Auth;
using BigBall.Api.Data;
using BigBall.Domain.Entities;
using BigBall.Shared.Dtos;

namespace BigBall.Api.Endpoints;

/// <summary>
/// STUB — substituir por Supabase JWT conforme TechSpec §4.3.
/// Qualquer email/senha não-vazio autentica. joao.pereira@gmail.com resolve para o usuário seedado.
/// </summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", (LoginRequest req, InMemoryStore store, StubJwtIssuer issuer) =>
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            {
                return Results.BadRequest(new { error = "Email e senha são obrigatórios." });
            }

            var profile = store.FindProfileByEmail(req.Email);
            if (profile is null)
            {
                // Create an ad-hoc profile so any email "works" in the stub.
                profile = new Profile
                {
                    Id = Guid.NewGuid(),
                    Email = req.Email,
                    DisplayName = DeriveDisplayName(req.Email)
                };
                store.Profiles.TryAdd(profile.Id, profile);
            }

            var token = issuer.Issue(profile.Id, profile.Email, profile.DisplayName);
            return Results.Ok(new LoginResponse(
                token,
                new ProfileDto(profile.Id, profile.DisplayName, null)));
        })
        .AllowAnonymous()
        .WithName("Login");

        return app;
    }

    private static string DeriveDisplayName(string email)
    {
        var at = email.IndexOf('@');
        var local = at > 0 ? email[..at] : email;
        var parts = local.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
    }
}
