using System.Security.Claims;
using BigBall.Api.Data;
using BigBall.Domain.Scoring;
using BigBall.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Endpoints;

public static class PoolsEndpoints
{
    public static IEndpointRouteBuilder MapPoolsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pools").RequireAuthorization().WithTags("Pools");

        group.MapGet("/mine", async (ClaimsPrincipal user, BigBallDbContext db, RankingService ranking, CancellationToken ct) =>
        {
            var userId = user.RequireUserId();
            var myMemberships = await db.PoolMemberships.Where(m => m.UserId == userId).ToListAsync(ct);
            var now = DateTime.UtcNow;

            var result = new List<MyPoolDto>();
            foreach (var m in myMemberships)
            {
                var pool = await db.Pools.FirstAsync(p => p.Id == m.PoolId, ct);
                var rows = await ranking.BuildRankingAsync(pool.Id, userId, ct);
                var myRow = rows.FirstOrDefault(r => r.IsMe);
                var leaderRow = rows.FirstOrDefault();
                var memberCount = await db.PoolMemberships.CountAsync(x => x.PoolId == pool.Id, ct);

                var nextMatch = await FindNextMatchAsync(db, now, ct);
                NextMatchDto? nextDto = null;
                if (nextMatch is not null)
                {
                    var myPred = await db.Predictions.FirstOrDefaultAsync(
                        p => p.UserId == userId && p.PoolId == pool.Id && p.MatchId == nextMatch.Id, ct);
                    nextDto = new NextMatchDto(
                        nextMatch.Id,
                        nextMatch.HomeCode,
                        nextMatch.AwayCode,
                        nextMatch.KickoffUtc,
                        nextMatch.GroupLabel,
                        myPred is null ? null : new ScoreDto(myPred.Home, myPred.Away));
                }

                result.Add(new MyPoolDto(
                    pool.Id,
                    pool.Name,
                    memberCount,
                    myRow?.Rank ?? 0,
                    myRow?.Points ?? 0,
                    leaderRow is null
                        ? new LeaderDto(Guid.Empty, "—", 0)
                        : new LeaderDto(leaderRow.UserId, leaderRow.Name, leaderRow.Points),
                    nextDto));
            }

            return Results.Ok(result);
        })
        .WithName("GetMyPools");

        group.MapGet("/{poolId:guid}", async (Guid poolId, ClaimsPrincipal user, BigBallDbContext db, RankingService ranking, CancellationToken ct) =>
        {
            var userId = user.RequireUserId();
            var pool = await db.Pools.FirstOrDefaultAsync(p => p.Id == poolId, ct);
            if (pool is null)
            {
                return Results.NotFound(new { error = "Bolão não encontrado." });
            }

            var isMember = await db.PoolMemberships.AnyAsync(m => m.PoolId == poolId && m.UserId == userId, ct);
            if (!isMember)
            {
                return Results.Forbid();
            }

            var rows = await ranking.BuildRankingAsync(poolId, userId, ct);
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

    private static async Task<BigBall.Domain.Entities.Match?> FindNextMatchAsync(BigBallDbContext db, DateTime now, CancellationToken ct)
    {
        return await db.Matches
            .Where(m => m.KickoffUtc > now)
            .OrderBy(m => m.KickoffUtc)
            .FirstOrDefaultAsync(ct);
    }
}
