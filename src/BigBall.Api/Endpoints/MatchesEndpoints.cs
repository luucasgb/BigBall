using System.Security.Claims;
using BigBall.Api.Data;
using BigBall.Shared.Dtos;

namespace BigBall.Api.Endpoints;

public static class MatchesEndpoints
{
    public static IEndpointRouteBuilder MapMatchesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/matches").RequireAuthorization().WithTags("Matches");

        group.MapGet("/{matchId:guid}", (Guid matchId, Guid poolId, ClaimsPrincipal user, InMemoryStore store) =>
        {
            var userId = user.RequireUserId();

            if (!store.Matches.TryGetValue(matchId, out var match))
            {
                return Results.NotFound(new { error = "Partida não encontrada." });
            }
            if (!store.Pools.ContainsKey(poolId))
            {
                return Results.NotFound(new { error = "Bolão não encontrado." });
            }
            var isMember = store.MembersOf(poolId).Any(m => m.UserId == userId);
            if (!isMember)
            {
                return Results.Forbid();
            }

            var pred = store.FindPrediction(userId, poolId, matchId);
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
