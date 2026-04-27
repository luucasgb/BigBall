using System.Security.Claims;
using BigBall.Api.Data;
using BigBall.Domain.Entities;
using BigBall.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Endpoints;

public static class PredictionsEndpoints
{
    public static IEndpointRouteBuilder MapPredictionsEndpoints(this IEndpointRouteBuilder app)
    {
        // PRD 4.7: server-side enforcement — PUT returns 409 LOCKED if UtcNow >= LockUtc.
        app.MapPut("/api/pools/{poolId:guid}/matches/{matchId:guid}/prediction",
            async (Guid poolId, Guid matchId, UpsertPredictionRequest req, ClaimsPrincipal user, BigBallDbContext db, CancellationToken ct) =>
        {
            var userId = user.RequireUserId();

            if (req.Home < 0 || req.Home > 20 || req.Away < 0 || req.Away > 20)
            {
                return Results.BadRequest(new { error = "Placar fora do intervalo permitido (0–20)." });
            }
            if (!await db.Pools.AnyAsync(p => p.Id == poolId, ct))
            {
                return Results.NotFound(new { error = "Bolão não encontrado." });
            }
            var match = await db.Matches.FirstOrDefaultAsync(m => m.Id == matchId, ct);
            if (match is null)
            {
                return Results.NotFound(new { error = "Partida não encontrada." });
            }
            var isMember = await db.PoolMemberships.AnyAsync(m => m.PoolId == poolId && m.UserId == userId, ct);
            if (!isMember)
            {
                return Results.Forbid();
            }
            if (match.IsLocked(DateTime.UtcNow))
            {
                return Results.Json(LockedError.Default, statusCode: StatusCodes.Status409Conflict);
            }

            var existing = await db.Predictions.FirstOrDefaultAsync(
                p => p.UserId == userId && p.PoolId == poolId && p.MatchId == matchId, ct);
            Prediction saved;
            if (existing is null)
            {
                saved = new Prediction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PoolId = poolId,
                    MatchId = matchId,
                    Home = req.Home,
                    Away = req.Away,
                    PenaltyWinnerCode = req.PenaltyWinnerCode
                };
                db.Predictions.Add(saved);
            }
            else
            {
                existing.Home = req.Home;
                existing.Away = req.Away;
                existing.PenaltyWinnerCode = req.PenaltyWinnerCode;
                existing.UpdatedUtc = DateTime.UtcNow;
                saved = existing;
            }

            await db.SaveChangesAsync(ct);

            var dto = new PredictionDto(saved.MatchId, saved.Home, saved.Away, saved.PenaltyWinnerCode, saved.UpdatedUtc);
            return Results.Ok(dto);
        })
        .RequireAuthorization()
        .WithTags("Predictions")
        .WithName("UpsertPrediction");

        return app;
    }
}
