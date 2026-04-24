using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Abstractions;

public interface IPredictionsApi
{
    /// <summary>
    /// Persists a prediction. Throws <see cref="PredictionLockedException"/> when the server
    /// returns 409 (lock window already started — PRD §4.7).
    /// </summary>
    Task<PredictionDto> UpsertAsync(
        Guid poolId,
        Guid matchId,
        UpsertPredictionRequest request,
        CancellationToken ct = default);
}

public sealed class PredictionLockedException : Exception
{
    public PredictionLockedException(string message) : base(message) { }
}
