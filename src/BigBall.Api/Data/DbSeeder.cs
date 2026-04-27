using BigBall.Domain.Entities;
using BigBall.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(BigBallDbContext db, CancellationToken ct = default)
    {
        if (await db.Pools.AnyAsync(ct))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var joaoId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var anaId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        db.Profiles.AddRange(
            new Profile { Id = joaoId, Email = "joao.pereira@gmail.com", DisplayName = "João Pereira", CreateDate = now.AddYears(-1) },
            new Profile { Id = anaId, Email = "ana.luz@gmail.com", DisplayName = "Ana Luz", CreateDate = now.AddYears(-1) });

        var familiaId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var trampoId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
        db.Pools.AddRange(
            new Pool
            {
                Id = familiaId,
                Name = "Família Silva 2026",
                Description = "Bolão clássico da família.",
                Visibility = PoolVisibility.Private,
                InviteCode = "FAMSILVA26",
                AdminUserId = anaId,
                PrizeDescription = "Troféu 3D + pizza",
                EntryCost = "R$ 20 (off-platform)",
                CreatedUtc = now.AddDays(-30)
            },
            new Pool
            {
                Id = trampoId,
                Name = "Trampo TechCo",
                Description = "Bolão da galera do trabalho.",
                Visibility = PoolVisibility.Private,
                InviteCode = "TECHCO26",
                AdminUserId = joaoId,
                PrizeDescription = "Happy hour",
                CreatedUtc = now.AddDays(-20)
            });

        db.PoolMemberships.AddRange(
            new PoolMembership { Id = Guid.NewGuid(), PoolId = familiaId, UserId = anaId, Role = MembershipRole.Admin, JoinedUtc = now.AddDays(-30) },
            new PoolMembership { Id = Guid.NewGuid(), PoolId = familiaId, UserId = joaoId, Role = MembershipRole.Member, JoinedUtc = now.AddDays(-25) },
            new PoolMembership { Id = Guid.NewGuid(), PoolId = trampoId, UserId = joaoId, Role = MembershipRole.Admin, JoinedUtc = now.AddDays(-20) },
            new PoolMembership { Id = Guid.NewGuid(), PoolId = trampoId, UserId = anaId, Role = MembershipRole.Member, JoinedUtc = now.AddDays(-15) });

        await db.SaveChangesAsync(ct);
    }
}
