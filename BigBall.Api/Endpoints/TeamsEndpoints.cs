using System.Security.Claims;
using BigBall.Api.Data;
using BigBall.Api.Integrations.FlashScore;
using BigBall.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Endpoints;

public static class TeamsEndpoints
{
    public static IEndpointRouteBuilder MapTeamsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/teams").WithTags("Teams");

        group.MapGet("", async (BigBallDbContext db, HttpContext httpContext, CancellationToken ct) =>
        {
            var rows = await db.Teams
                .AsNoTracking()
                .OrderBy(t => t.Code)
                .Select(t => new TeamDto(t.Code, t.DisplayName, t.BadgeUrl, t.BadgeUrlSmall, t.CountryImageUrl))
                .ToListAsync(ct)
                .ConfigureAwait(false);

            httpContext.Response.Headers.CacheControl = "public, max-age=3600";
            return Results.Ok(rows);
        })
        .AllowAnonymous()
        .WithName("ListTeams");

        var admin = app.MapGroup("/api/admin/teams").RequireAuthorization().WithTags("Teams");

        admin.MapPost("/discover-badges-wc2026", async (
                ClaimsPrincipal user,
                BigBallDbContext db,
                FlashScoreTeamSearchService search,
                CancellationToken ct) =>
            {
                var userId = user.RequireUserId();
                var profile = await db.Profiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == userId, ct)
                    .ConfigureAwait(false);
                if (profile is null || !profile.IsPlatformAdmin)
                {
                    return Results.Forbid();
                }

                var report = await search.SeedWorldCup2026TeamsAsync(db, ct).ConfigureAwait(false);
                return Results.Ok(report);
            })
            .WithName("DiscoverWorldCup2026TeamBadges");

        return app;
    }
}
