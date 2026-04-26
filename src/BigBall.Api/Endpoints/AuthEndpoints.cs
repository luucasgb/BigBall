using BigBall.Api.Auth;
using BigBall.Api.Data;
using BigBall.Domain.Entities;
using BigBall.Domain.Enums;
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

            // Stub: every logged-in user must belong to at least the two seeded pools so screens like
            // /predict (which calls /api/pools/mine) always have bolões for local development.
            EnsureStubDefaultPoolMemberships(store, profile.Id);

            var token = issuer.Issue(profile.Id, profile.Email, profile.DisplayName);
            return Results.Ok(new LoginResponse(
                token,
                new ProfileDto(profile.Id, profile.DisplayName, null)));
        })
        .AllowAnonymous()
        .WithName("Login");

        return app;
    }

    private static void EnsureStubDefaultPoolMemberships(InMemoryStore store, Guid userId)
    {
        if (store.MembershipsOf(userId).Any())
        {
            return;
        }

        var joined = DateTime.UtcNow;
        foreach (var poolId in new[] { SeedData.FamiliaPoolId, SeedData.TrampoPoolId })
        {
            if (!store.Pools.ContainsKey(poolId))
            {
                continue;
            }

            var id = Guid.NewGuid();
            store.Memberships.TryAdd(id, new PoolMembership
            {
                Id = id,
                PoolId = poolId,
                UserId = userId,
                Role = MembershipRole.Member,
                JoinedUtc = joined
            });
        }
    }

    private static string DeriveDisplayName(string email)
    {
        var at = email.IndexOf('@');
        var local = at > 0 ? email[..at] : email;
        var parts = local.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
    }
}
