using System.Security.Claims;
using BigBall.Api.Data;
using BigBall.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Endpoints;

public static class MatchesEndpoints
{
    public static IEndpointRouteBuilder MapMatchesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/matches").RequireAuthorization().WithTags("Matches");

        group.MapGet("/{matchId:guid}", async (Guid matchId, Guid poolId, ClaimsPrincipal user, BigBallDbContext db, CancellationToken ct) =>
        {
            var userId = user.RequireUserId();

            var match = await db.Matches.FirstOrDefaultAsync(m => m.Id == matchId, ct);
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
}
