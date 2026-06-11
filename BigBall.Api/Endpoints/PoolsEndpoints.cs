using System.Security.Claims;
using System.Security.Cryptography;
using BigBall.Api.Data;
using BigBall.Domain.Entities;
using BigBall.Domain.Enums;
using BigBall.Domain.Scoring;
using BigBall.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Endpoints;

public static class PoolsEndpoints
{
    public static IEndpointRouteBuilder MapPoolsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pools").RequireAuthorization().WithTags("Pools");

        group.MapPost("", async (CreatePoolRequest req, ClaimsPrincipal user, BigBallDbContext db, CancellationToken ct) =>
        {
            var userId = user.RequireUserId();

            var name = (req.Name ?? string.Empty).Trim();
            var prize = (req.PrizeDescription ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(name))
            {
                return Results.BadRequest(new { error = "Nome é obrigatório." });
            }
            if (string.IsNullOrEmpty(prize))
            {
                return Results.BadRequest(new { error = "Premiação é obrigatória." });
            }
            if (name.Length > 140)
            {
                return Results.BadRequest(new { error = "Nome excede o tamanho máximo (140 caracteres)." });
            }
            var description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
            if (description is { Length: > 1000 })
            {
                return Results.BadRequest(new { error = "Descrição excede o tamanho máximo (1000 caracteres)." });
            }
            if (prize.Length > 300)
            {
                return Results.BadRequest(new { error = "Premiação excede o tamanho máximo (300 caracteres)." });
            }
            var entryCost = string.IsNullOrWhiteSpace(req.EntryCost) ? null : req.EntryCost.Trim();
            if (entryCost is { Length: > 120 })
            {
                return Results.BadRequest(new { error = "Custo de entrada excede o tamanho máximo (120 caracteres)." });
            }

            if (!Enum.TryParse<PoolVisibility>(req.Visibility?.Trim(), ignoreCase: true, out var visibility))
            {
                return Results.BadRequest(new { error = "Visibilidade inválida. Use Public ou Private." });
            }

            string? inviteCode = null;
            if (visibility == PoolVisibility.Private)
            {
                const int maxAttempts = 32;
                for (var attempt = 0; attempt < maxAttempts; attempt++)
                {
                    var code = GenerateInviteCode();
                    var taken = await db.Pools.AnyAsync(p => p.InviteCode == code, ct);
                    if (!taken)
                    {
                        inviteCode = code;
                        break;
                    }
                }
                if (inviteCode is null)
                {
                    return Results.Problem("Não foi possível gerar um código de convite único.");
                }
            }

            var poolId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            db.Pools.Add(new Pool
            {
                Id = poolId,
                Name = name,
                Description = description,
                Visibility = visibility,
                InviteCode = inviteCode,
                AdminUserId = userId,
                PrizeDescription = prize,
                EntryCost = entryCost,
                CreatedUtc = now
            });
            db.PoolMemberships.Add(new PoolMembership
            {
                Id = Guid.NewGuid(),
                PoolId = poolId,
                UserId = userId,
                Role = MembershipRole.Admin,
                JoinedUtc = now
            });

            await db.SaveChangesAsync(ct);
            return Results.Ok(new CreatePoolResponse(poolId, inviteCode));
        })
        .WithName("CreatePool");

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

        group.MapGet("/public", async (ClaimsPrincipal user, BigBallDbContext db, CancellationToken ct) =>
        {
            var userId = user.RequireUserId();

            var pools = await db.Pools
                .AsNoTracking()
                .Where(p => p.Visibility == PoolVisibility.Public)
                .OrderByDescending(p => p.CreatedUtc)
                .ToListAsync(ct);

            var memberCounts = await db.PoolMemberships
                .GroupBy(m => m.PoolId)
                .Select(g => new { PoolId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PoolId, x => x.Count, ct);

            var myPoolIds = (await db.PoolMemberships
                .Where(m => m.UserId == userId)
                .Select(m => m.PoolId)
                .ToListAsync(ct)).ToHashSet();

            var adminIds = pools.Select(p => p.AdminUserId).Distinct().ToList();
            var adminNames = await db.Profiles
                .Where(p => adminIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.DisplayName, ct);

            var result = pools.Select(p => new PublicPoolDto(
                p.Id,
                p.Name,
                p.Description,
                memberCounts.GetValueOrDefault(p.Id, 0),
                p.PrizeDescription,
                p.EntryCost,
                adminNames.GetValueOrDefault(p.AdminUserId, "—"),
                myPoolIds.Contains(p.Id))).ToList();

            return Results.Ok(result);
        })
        .WithName("GetPublicPools");

        group.MapPost("/{poolId:guid}/join", async (Guid poolId, ClaimsPrincipal user, BigBallDbContext db, CancellationToken ct) =>
        {
            var userId = user.RequireUserId();

            var pool = await db.Pools.FirstOrDefaultAsync(
                p => p.Id == poolId && p.Visibility == PoolVisibility.Public, ct);
            if (pool is null)
            {
                return Results.NotFound(new { error = "Bolão público não encontrado." });
            }

            var alreadyMember = await db.PoolMemberships.AnyAsync(
                m => m.PoolId == pool.Id && m.UserId == userId, ct);
            if (alreadyMember)
            {
                return Results.Conflict(new { error = "Você já participa deste bolão." });
            }

            db.PoolMemberships.Add(new PoolMembership
            {
                Id = Guid.NewGuid(),
                PoolId = pool.Id,
                UserId = userId,
                Role = MembershipRole.Member,
                JoinedUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync(ct);
            return Results.Ok(new JoinPoolResponse(pool.Id, pool.Name));
        })
        .WithName("JoinPublicPool");

        group.MapPost("/join", async (JoinPoolRequest req, ClaimsPrincipal user, BigBallDbContext db, CancellationToken ct) =>
        {
            var userId = user.RequireUserId();
            var raw = (req.InviteCode ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(raw))
            {
                return Results.BadRequest(new { error = "Código de convite é obrigatório." });
            }
            if (raw.Length > 32)
            {
                return Results.BadRequest(new { error = "Código de convite inválido." });
            }

            var normalized = raw.ToUpperInvariant();
            var pool = await db.Pools.FirstOrDefaultAsync(
                p => p.InviteCode == normalized
                     && p.Visibility == PoolVisibility.Private
                     && p.InviteCode != null,
                ct);
            if (pool is null)
            {
                return Results.NotFound(new { error = "Código de convite inválido." });
            }

            var alreadyMember = await db.PoolMemberships.AnyAsync(
                m => m.PoolId == pool.Id && m.UserId == userId, ct);
            if (alreadyMember)
            {
                return Results.Conflict(new { error = "Você já participa deste bolão." });
            }

            var now = DateTime.UtcNow;
            db.PoolMemberships.Add(new PoolMembership
            {
                Id = Guid.NewGuid(),
                PoolId = pool.Id,
                UserId = userId,
                Role = MembershipRole.Member,
                JoinedUtc = now
            });
            await db.SaveChangesAsync(ct);
            return Results.Ok(new JoinPoolResponse(pool.Id, pool.Name));
        })
        .WithName("JoinPoolByInvite");

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

        group.MapGet("/{poolId:guid}/matches", async (Guid poolId, ClaimsPrincipal user, BigBallDbContext db, CancellationToken ct) =>
        {
            var userId = user.RequireUserId();
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

            var matches = await db.Matches
                .AsNoTracking()
                .OrderBy(m => m.KickoffUtc)
                .ToListAsync(ct);

            var myPredictions = await db.Predictions
                .AsNoTracking()
                .Where(p => p.UserId == userId && p.PoolId == poolId)
                .ToDictionaryAsync(p => p.MatchId, ct);

            var rows = matches
                .Select(m =>
                {
                    myPredictions.TryGetValue(m.Id, out var pred);
                    return new PoolMatchRowDto(
                        m.Id,
                        m.Phase.ToString(),
                        m.GroupLabel,
                        m.HomeCode,
                        m.AwayCode,
                        m.KickoffUtc,
                        m.Status.ToString(),
                        m.ReferenceHome,
                        m.ReferenceAway,
                        pred is null ? null : new ScoreDto(pred.Home, pred.Away));
                })
                .ToList();

            return Results.Ok(rows);
        })
        .WithName("GetPoolMatches");

        return app;
    }

    /// <summary>Uppercase alphanumeric without ambiguous I, O, 0, 1.</summary>
    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = new byte[8];
        RandomNumberGenerator.Fill(bytes);
        return string.Create(8, bytes, static (span, b) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = chars[b[i] % chars.Length];
            }
        });
    }

    private static async Task<BigBall.Domain.Entities.Match?> FindNextMatchAsync(BigBallDbContext db, DateTime now, CancellationToken ct)
    {
        return await db.Matches
            .Where(m => m.KickoffUtc > now)
            .OrderBy(m => m.KickoffUtc)
            .FirstOrDefaultAsync(ct);
    }
}
