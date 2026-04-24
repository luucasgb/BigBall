using System.Collections.Concurrent;
using BigBall.Domain.Entities;

namespace BigBall.Api.Data;

/// <summary>
/// Thread-safe in-memory source of truth for the stub API.
/// Composite reads (e.g. ranking) take the <see cref="RankingLock"/> briefly.
/// </summary>
public sealed class InMemoryStore
{
    public ConcurrentDictionary<Guid, Profile> Profiles { get; } = new();
    public ConcurrentDictionary<Guid, Pool> Pools { get; } = new();
    public ConcurrentDictionary<Guid, PoolMembership> Memberships { get; } = new();
    public ConcurrentDictionary<Guid, Match> Matches { get; } = new();
    public ConcurrentDictionary<Guid, Prediction> Predictions { get; } = new();

    /// <summary>Serialize reads that aggregate multiple dictionaries.</summary>
    public ReaderWriterLockSlim RankingLock { get; } = new(LockRecursionPolicy.NoRecursion);

    public Profile? FindProfileByEmail(string email) =>
        Profiles.Values.FirstOrDefault(p => string.Equals(p.Email, email, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<PoolMembership> MembershipsOf(Guid userId) =>
        Memberships.Values.Where(m => m.UserId == userId);

    public IEnumerable<PoolMembership> MembersOf(Guid poolId) =>
        Memberships.Values.Where(m => m.PoolId == poolId);

    public Prediction? FindPrediction(Guid userId, Guid poolId, Guid matchId) =>
        Predictions.Values.FirstOrDefault(p => p.UserId == userId && p.PoolId == poolId && p.MatchId == matchId);
}
