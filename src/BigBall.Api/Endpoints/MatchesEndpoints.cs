using System.Security.Claims;
using BigBall.Api.Data;
using BigBall.Domain.Entities;
using BigBall.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Endpoints;

public static class MatchesEndpoints
{
    public static IEndpointRouteBuilder MapMatchesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/matches").RequireAuthorization().WithTags("Matches");

        group.MapGet("", async (DateTime? fromUtc, DateTime? toUtc, BigBallDbContext db, CancellationToken ct) =>
        {
            var from = fromUtc ?? DateTime.MinValue;
            var to = toUtc ?? DateTime.MaxValue;
            if (from.Kind != DateTimeKind.Utc)
            {
                from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            }

            if (to.Kind != DateTimeKind.Utc)
            {
                to = DateTime.SpecifyKind(to, DateTimeKind.Utc);
            }

            var matches = await db.Matches
                .AsNoTracking()
                .Include(m => m.HostCity)
                .Where(m => m.KickoffUtc >= from && m.KickoffUtc <= to)
                .OrderBy(m => m.KickoffUtc)
                .ToListAsync(ct);
            var rows = matches
                .Select(m => new MatchCalendarRowDto(
                    m.Id,
                    m.Phase.ToString(),
                    m.GroupLabel,
                    m.HomeCode,
                    m.AwayCode,
                    m.KickoffUtc,
                    m.Venue,
                    MapHostCity(m.HostCity),
                    m.Status.ToString()))
                .ToList();
            return Results.Ok(rows);
        })
        .WithName("ListMatchesInRange");

        group.MapGet("/{matchId:guid}", async (Guid matchId, Guid poolId, ClaimsPrincipal user, BigBallDbContext db, CancellationToken ct) =>
        {
            var userId = user.RequireUserId();

            var match = await db.Matches
                .AsNoTracking()
                .Include(m => m.HostCity)
                .FirstOrDefaultAsync(m => m.Id == matchId, ct);
            if (match is null)
            {
                return Results.NotFound(new { error = "Partida não encontrada." });
            }
            var poolExists = await db.Pools.AnyAsync(p => p.Id == poolId, ct);
            if (!poolExists)
            {
                return Results.NotFound(new { error = "Bolão não encontrado." });
            }
            var isMember = await db.PoolMemberships.AnyAsync(m => m.PoolId == poolId && m.UserId == userId, ct);
            if (!isMember)
            {
                return Results.Forbid();
            }

            var pred = await db.Predictions.FirstOrDefaultAsync(
                p => p.UserId == userId && p.PoolId == poolId && p.MatchId == matchId, ct);
            var predDto = pred is null
                ? null
                : new PredictionDto(pred.MatchId, pred.Home, pred.Away, pred.PenaltyWinnerCode, pred.UpdatedUtc);

            var dto = new MatchDetailDto(
                match.Id,
                match.Phase.ToString(),
                match.GroupLabel,
                match.HomeCode,
                match.AwayCode,
                match.KickoffUtc,
                match.LockUtc,
                match.Venue,
                MapHostCity(match.HostCity),
                match.Status.ToString(),
                match.ReferenceHome,
                match.ReferenceAway,
                match.WentToPenalties,
                match.PenaltyWinnerCode,
                predDto);

            return Results.Ok(dto);
        })
        .WithName("GetMatchDetail");

        return app;
    }

    private static HostCityDto? MapHostCity(HostCity? h) =>
        h is null
            ? null
            : new HostCityDto(h.Id, h.CityName, h.Country, h.VenueName, h.RegionCluster, h.AirportCode);
}
