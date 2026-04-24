using System.Security.Claims;
using BigBall.Api.Data;
using BigBall.Domain.Entities;
using BigBall.Domain.Scoring;
using BigBall.Shared.Dtos;

namespace BigBall.Api.Endpoints;

public static class PoolsEndpoints
{
    public static IEndpointRouteBuilder MapPoolsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pools").RequireAuthorization().WithTags("Pools");

        group.MapGet("/mine", (ClaimsPrincipal user, InMemoryStore store, RankingService ranking) =>
        {
            var userId = user.RequireUserId();
            var myMemberships = store.MembershipsOf(userId).ToList();
            var now = DateTime.UtcNow;

            var result = myMemberships.Select(m =>
            {
                var pool = store.Pools[m.PoolId];
                var rows = ranking.BuildRanking(pool.Id, userId);
                var myRow = rows.FirstOrDefault(r => r.IsMe);
                var leaderRow = rows.FirstOrDefault();
                var memberCount = store.MembersOf(pool.Id).Count();

                var nextMatch = FindNextMatch(store, now);
                NextMatchDto? nextDto = null;
                if (nextMatch is not null)
                {
                    var myPred = store.FindPrediction(userId, pool.Id, nextMatch.Id);
                    nextDto = new NextMatchDto(
                        nextMatch.Id,
                        nextMatch.HomeCode,
                        nextMatch.AwayCode,
                        nextMatch.KickoffUtc,
                        nextMatch.GroupLabel,
                        myPred is null ? null : new ScoreDto(myPred.Home, myPred.Away));
                }

                return new MyPoolDto(
                    pool.Id,
                    pool.Name,
                    memberCount,
                    myRow?.Rank ?? 0,
                    myRow?.Points ?? 0,
                    leaderRow is null
                        ? new LeaderDto(Guid.Empty, "—", 0)
                        : new LeaderDto(leaderRow.UserId, leaderRow.Name, leaderRow.Points),
                    nextDto);
            }).ToList();

            return Results.Ok(result);
        })
        .WithName("GetMyPools");

        group.MapGet("/{poolId:guid}", (Guid poolId, ClaimsPrincipal user, InMemoryStore store, RankingService ranking) =>
        {
            var userId = user.RequireUserId();
            if (!store.Pools.TryGetValue(poolId, out var pool))
            {
                return Results.NotFound(new { error = "Bolão não encontrado." });
            }

            var isMember = store.MembersOf(poolId).Any(m => m.UserId == userId);
            if (!isMember)
            {
                return Results.Forbid();
            }

            var rows = ranking.BuildRanking(poolId, userId);
            var myRow = rows.FirstOrDefault(r => r.IsMe);
            var leaderRow = rows.FirstOrDefault();

            return Results.Ok(new PoolDetailDto(
                pool.Id,
                pool.Name,
                pool.Description,
                pool.Visibility.ToString(),
                pool.InviteCode,
                pool.PrizeDescription,
                pool.EntryCost,
                myRow?.Rank ?? 0,
                myRow?.Points ?? 0,
                leaderRow is null
                    ? new LeaderDto(Guid.Empty, "—", 0)
                    : new LeaderDto(leaderRow.UserId, leaderRow.Name, leaderRow.Points),
                rows));
        })
        .WithName("GetPoolDetail");

        return app;
    }

    private static Match? FindNextMatch(InMemoryStore store, DateTime now)
    {
        return store.Matches.Values
            .Where(m => m.KickoffUtc > now)
            .OrderBy(m => m.KickoffUtc)
            .FirstOrDefault();
    }
}
