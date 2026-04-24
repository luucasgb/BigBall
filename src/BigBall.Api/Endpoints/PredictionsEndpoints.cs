using System.Security.Claims;
using BigBall.Api.Data;
using BigBall.Domain.Entities;
using BigBall.Shared.Dtos;

namespace BigBall.Api.Endpoints;

public static class PredictionsEndpoints
{
    public static IEndpointRouteBuilder MapPredictionsEndpoints(this IEndpointRouteBuilder app)
    {
        // PRD 4.7: server-side enforcement — PUT returns 409 LOCKED if UtcNow >= LockUtc.
        app.MapPut("/api/pools/{poolId:guid}/matches/{matchId:guid}/prediction",
            (Guid poolId, Guid matchId, UpsertPredictionRequest req, ClaimsPrincipal user, InMemoryStore store) =>
        {
            var userId = user.RequireUserId();

            if (req.Home < 0 || req.Home > 20 || req.Away < 0 || req.Away > 20)
            {
                return Results.BadRequest(new { error = "Placar fora do intervalo permitido (0–20)." });
            }
            if (!store.Pools.ContainsKey(poolId))
            {
                return Results.NotFound(new { error = "Bolão não encontrado." });
            }
            if (!store.Matches.TryGetValue(matchId, out var match))
            {
                return Results.NotFound(new { error = "Partida não encontrada." });
            }
            var isMember = store.MembersOf(poolId).Any(m => m.UserId == userId);
            if (!isMember)
            {
                return Results.Forbid();
            }
            if (match.IsLocked(DateTime.UtcNow))
            {
                return Results.Json(LockedError.Default, statusCode: StatusCodes.Status409Conflict);
            }

            var existing = store.FindPrediction(userId, poolId, matchId);
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
                store.Predictions.TryAdd(saved.Id, saved);
            }
            else
            {
                existing.Home = req.Home;
                existing.Away = req.Away;
                existing.PenaltyWinnerCode = req.PenaltyWinnerCode;
                existing.UpdatedUtc = DateTime.UtcNow;
                saved = existing;
            }

            var dto = new PredictionDto(saved.MatchId, saved.Home, saved.Away, saved.PenaltyWinnerCode, saved.UpdatedUtc);
            return Results.Ok(dto);
        })
        .RequireAuthorization()
        .WithTags("Predictions")
        .WithName("UpsertPrediction");

        return app;
    }
}
