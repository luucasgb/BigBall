using BigBall.Domain.Entities;
using BigBall.Domain.Enums;

namespace BigBall.Api.Data;

/// <summary>
/// Populates <see cref="InMemoryStore"/> with the fixtures the 4 priority screens need
/// to render realistic content matching the handoff mockups.
/// </summary>
public static class SeedData
{
    // Deterministic UUIDs so the current user mapping stays stable across restarts.
    private static readonly Guid JoaoId  = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AnaId   = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid BrunoId = new("33333333-3333-3333-3333-333333333333");
    private static readonly Guid CarlaId = new("44444444-4444-4444-4444-444444444444");
    private static readonly Guid DiegoId = new("55555555-5555-5555-5555-555555555555");

    private static readonly Guid FamiliaPoolId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
    private static readonly Guid TrampoPoolId  = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");

    private static readonly Guid ArgMexId       = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public static void Populate(InMemoryStore store)
    {
        SeedProfiles(store);
        SeedPools(store);
        SeedMemberships(store);
        SeedMatches(store);
        SeedPredictions(store);
    }

    /// <summary>Exposed so the auth endpoint can resolve the seeded "current user" João.</summary>
    public static Guid JoaoUserId => JoaoId;

    private static void SeedProfiles(InMemoryStore s)
    {
        Add(s, new Profile { Id = JoaoId,  Email = "joao.pereira@gmail.com", DisplayName = "João Pereira" });
        Add(s, new Profile { Id = AnaId,   Email = "ana.luz@gmail.com",       DisplayName = "Ana Luz" });
        Add(s, new Profile { Id = BrunoId, Email = "bruno.r@gmail.com",       DisplayName = "Bruno R." });
        Add(s, new Profile { Id = CarlaId, Email = "carla.m@gmail.com",       DisplayName = "Carla M." });
        Add(s, new Profile { Id = DiegoId, Email = "diego.f@gmail.com",       DisplayName = "Diego F." });

        static void Add(InMemoryStore s, Profile p) => s.Profiles.TryAdd(p.Id, p);
    }

    private static void SeedPools(InMemoryStore s)
    {
        s.Pools.TryAdd(FamiliaPoolId, new Pool
        {
            Id = FamiliaPoolId,
            Name = "Família Silva 2026",
            Description = "Bolão clássico da família — prêmio: troféu 3D impresso.",
            Visibility = PoolVisibility.Private,
            InviteCode = "FAMSILVA26",
            AdminUserId = AnaId,
            PrizeDescription = "Troféu 3D + rodada de pizza",
            EntryCost = "R$ 20 (off-platform)"
        });
        s.Pools.TryAdd(TrampoPoolId, new Pool
        {
            Id = TrampoPoolId,
            Name = "Trampo TechCo",
            Description = "Bolão da galera do trabalho.",
            Visibility = PoolVisibility.Private,
            InviteCode = "TECHCO26",
            AdminUserId = JoaoId,
            PrizeDescription = "Happy hour pago pelo último colocado",
            EntryCost = null
        });
    }

    private static void SeedMemberships(InMemoryStore s)
    {
        // Família Silva: João + Ana (admin) + Bruno + Carla + Diego + 7 generated "fillers"
        AddMember(s, FamiliaPoolId, AnaId,   MembershipRole.Admin);
        AddMember(s, FamiliaPoolId, JoaoId,  MembershipRole.Member);
        AddMember(s, FamiliaPoolId, BrunoId, MembershipRole.Member);
        AddMember(s, FamiliaPoolId, CarlaId, MembershipRole.Member);
        AddMember(s, FamiliaPoolId, DiegoId, MembershipRole.Member);
        AddFillerMembers(s, FamiliaPoolId, 7);

        // Trampo TechCo: João (admin) + others + 25 filler
        AddMember(s, TrampoPoolId, JoaoId,  MembershipRole.Admin);
        AddMember(s, TrampoPoolId, BrunoId, MembershipRole.Member);
        AddMember(s, TrampoPoolId, CarlaId, MembershipRole.Member);
        AddFillerMembers(s, TrampoPoolId, 25);

        static void AddMember(InMemoryStore s, Guid poolId, Guid userId, MembershipRole role)
        {
            var id = Guid.NewGuid();
            s.Memberships.TryAdd(id, new PoolMembership
            {
                Id = id,
                PoolId = poolId,
                UserId = userId,
                Role = role,
                JoinedUtc = DateTime.UtcNow.AddDays(-30)
            });
        }

        static void AddFillerMembers(InMemoryStore s, Guid poolId, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var fillerId = Guid.NewGuid();
                s.Profiles.TryAdd(fillerId, new Profile
                {
                    Id = fillerId,
                    Email = $"filler{i}.{poolId.ToString()[..4]}@bigball.local",
                    DisplayName = GenerateFillerName(i, poolId)
                });
                var membershipId = Guid.NewGuid();
                s.Memberships.TryAdd(membershipId, new PoolMembership
                {
                    Id = membershipId,
                    PoolId = poolId,
                    UserId = fillerId,
                    Role = MembershipRole.Member,
                    JoinedUtc = DateTime.UtcNow.AddDays(-15 + i)
                });
            }
        }
    }

    private static string GenerateFillerName(int i, Guid poolId)
    {
        string[] firstNames = { "Lucas", "Paula", "Renato", "Sofia", "Thiago", "Mariana", "Rafael", "Beatriz", "Pedro", "Larissa", "Marcos", "Isabela", "Gustavo", "Vitória", "Felipe", "Camila", "André", "Juliana", "Victor", "Helena", "Rodrigo", "Patricia", "Daniel", "Letícia", "Eduardo" };
        string[] lastInitials = { "A.", "B.", "C.", "D.", "E.", "F.", "G.", "H.", "I.", "J.", "K.", "L.", "M.", "N.", "O.", "P.", "R.", "S.", "T.", "V." };
        var salt = poolId.GetHashCode();
        return $"{firstNames[(i + Math.Abs(salt)) % firstNames.Length]} {lastInitials[(i + Math.Abs(salt / 3)) % lastInitials.Length]}";
    }

    private static void SeedMatches(InMemoryStore s)
    {
        var now = DateTime.UtcNow;

        // Finalizadas (com TR): ARG 2x1 CAN, BRA 1x1 POR, FRA 3x0 AUS
        AddMatch(s, MatchPhase.Groups, "A", "ARG", "CAN", now.AddDays(-2), status: MatchStatus.Finished, refH: 2, refA: 1);
        AddMatch(s, MatchPhase.Groups, "B", "BRA", "POR", now.AddDays(-2).AddHours(3), status: MatchStatus.Finished, refH: 1, refA: 1);
        AddMatch(s, MatchPhase.Groups, "C", "FRA", "AUS", now.AddDays(-1), status: MatchStatus.Finished, refH: 3, refA: 0);

        // Próximas (≈ 1h 47m): ARG×MEX, BRA×SUI
        AddMatch(s, MatchPhase.Groups, "A", "ARG", "MEX", now.AddMinutes(107), id: ArgMexId);
        AddMatch(s, MatchPhase.Groups, "B", "BRA", "SUI", now.AddMinutes(107).AddHours(3));

        // Já bloqueada (kickoff no passado em 10 min)
        AddMatch(s, MatchPhase.Groups, "D", "ESP", "MAR", now.AddMinutes(-10));

        // Futuras distantes
        AddMatch(s, MatchPhase.Groups, "E", "GER", "JPN", now.AddDays(3).AddHours(4));
        AddMatch(s, MatchPhase.Groups, "F", "ESP", "URU", now.AddDays(5));

        static void AddMatch(InMemoryStore s, MatchPhase phase, string? group, string home, string away,
            DateTime kickoff, MatchStatus status = MatchStatus.Scheduled, int? refH = null, int? refA = null,
            Guid? id = null)
        {
            var matchId = id ?? Guid.NewGuid();
            s.Matches.TryAdd(matchId, new Match
            {
                Id = matchId,
                Phase = phase,
                GroupLabel = group is null ? null : $"Grupo {group}",
                HomeCode = home,
                AwayCode = away,
                KickoffUtc = kickoff,
                Status = status,
                ReferenceHome = refH,
                ReferenceAway = refA,
                Venue = null
            });
        }
    }

    private static void SeedPredictions(InMemoryStore s)
    {
        // João's predictions — seed to produce the "267 pts" headline in Trampo TechCo
        // and "184 pts rank #3" in Família Silva.
        var argCan = s.Matches.Values.First(m => m.HomeCode == "ARG" && m.AwayCode == "CAN");
        var braPor = s.Matches.Values.First(m => m.HomeCode == "BRA" && m.AwayCode == "POR");
        var fraAus = s.Matches.Values.First(m => m.HomeCode == "FRA" && m.AwayCode == "AUS");
        var argMex = s.Matches.Values.First(m => m.HomeCode == "ARG" && m.AwayCode == "MEX");

        // Trampo TechCo
        AddPrediction(s, JoaoId, TrampoPoolId, argCan, 2, 1);         // exact 20
        AddPrediction(s, JoaoId, TrampoPoolId, braPor, 2, 2);         // draw, different score → 16
        AddPrediction(s, JoaoId, TrampoPoolId, fraAus, 3, 0);         // exact 20
        AddPrediction(s, JoaoId, TrampoPoolId, argMex, 2, 1);         // still lockable

        // Família Silva
        AddPrediction(s, JoaoId, FamiliaPoolId, argCan, 3, 2);        // winner + diff → 16
        AddPrediction(s, JoaoId, FamiliaPoolId, braPor, 1, 0);        // wrong (draw missed) → 5 (away side 0-matches? 0≠1, home 1≠1 no... 1==1 actually yes → 5)
        AddPrediction(s, JoaoId, FamiliaPoolId, fraAus, 2, 0);        // winner + away side 0 → 15

        // A few predictions for others so the ranking has realistic spread
        AddPrediction(s, AnaId,   TrampoPoolId,  argCan, 2, 0);       // winner + home side → 15
        AddPrediction(s, BrunoId, TrampoPoolId,  argCan, 1, 1);       // draw wrong → 0
        AddPrediction(s, CarlaId, TrampoPoolId,  argCan, 2, 1);       // exact → 20

        AddPrediction(s, AnaId,   FamiliaPoolId, argCan, 2, 1);       // exact → 20
        AddPrediction(s, BrunoId, FamiliaPoolId, argCan, 3, 1);       // winner only → 10
        AddPrediction(s, CarlaId, FamiliaPoolId, argCan, 2, 2);       // wrong (draw guess) → 5 (home 2==2)
        AddPrediction(s, DiegoId, FamiliaPoolId, argCan, 2, 1);       // exact → 20
        AddPrediction(s, AnaId,   FamiliaPoolId, braPor, 1, 1);       // exact → 20
        AddPrediction(s, DiegoId, FamiliaPoolId, fraAus, 3, 0);       // exact → 20

        static void AddPrediction(InMemoryStore s, Guid userId, Guid poolId, Match m, int home, int away)
        {
            var id = Guid.NewGuid();
            s.Predictions.TryAdd(id, new Prediction
            {
                Id = id,
                UserId = userId,
                PoolId = poolId,
                MatchId = m.Id,
                Home = home,
                Away = away
            });
        }
    }
}
