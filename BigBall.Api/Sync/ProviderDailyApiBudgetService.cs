using BigBall.Api.Data;
using BigBall.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Sync;

public interface IProviderDailyApiBudget
{
    Task<int> GetConsumedTodayUtcAsync(BigBallDbContext db, CancellationToken cancellationToken = default);

    Task AddConsumptionAsync(BigBallDbContext db, int httpGets, CancellationToken cancellationToken = default);
}

/// <summary>Persistent rolling counter keyed by UTC date for outbound HTTP budgeting.</summary>
public sealed class ProviderDailyApiBudgetService(ILogger<ProviderDailyApiBudgetService> logger)
    : IProviderDailyApiBudget
{
    public async Task<int> GetConsumedTodayUtcAsync(BigBallDbContext db,
        CancellationToken cancellationToken = default)
    {
        var day = DateOnly.FromDateTime(DateTime.UtcNow);
        var row =
            await db.ProviderDailyApiUsages.AsNoTracking()
                .FirstOrDefaultAsync(x => x.DayUtc == day, cancellationToken)
                .ConfigureAwait(false);

        return row?.HttpGetCount ?? 0;
    }

    public async Task AddConsumptionAsync(BigBallDbContext db, int httpGets,
        CancellationToken cancellationToken = default)
    {
        if (httpGets <= 0)
            return;

        var day = DateOnly.FromDateTime(DateTime.UtcNow);

        var row = await db.ProviderDailyApiUsages
            .SingleOrDefaultAsync(x => x.DayUtc == day, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
        {
            row = new ProviderDailyApiUsage { DayUtc = day, HttpGetCount = 0 };
            db.ProviderDailyApiUsages.Add(row);
        }

        row.HttpGetCount += httpGets;
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        logger.LogDebug("Recorded {Adds} outbound HTTP GETs (day {Day}, total={Total}).",
            httpGets, day, row.HttpGetCount);
    }
}
