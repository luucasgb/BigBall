using BigBall.Api.Data;
using BigBall.Api.Sync;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Tests.Sync;

public sealed class ProviderDailyApiBudgetServiceTests
{
    [Fact]
    public async Task AddConsumption_accumulates_same_utc_day()
    {
        await using var db = await CreateDbAsync();
        var svc = new ProviderDailyApiBudgetService(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ProviderDailyApiBudgetService>.Instance);

        await svc.AddConsumptionAsync(db, 2, default);
        await svc.AddConsumptionAsync(db, 3, default);

        var n = await svc.GetConsumedTodayUtcAsync(db, default);
        Assert.Equal(5, n);
    }

    private static async Task<BigBallDbContext> CreateDbAsync()
    {
        var opts = new DbContextOptionsBuilder<BigBallDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new BigBallDbContext(opts);
        await db.Database.EnsureCreatedAsync();
        return db;
    }
}
