using BigBall.Api.Configuration;
using BigBall.Api.Data;
using BigBall.Domain.Entities;
using BigBall.Domain.SportsData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BigBall.Api.Sync;

/// <summary>Polls persisted matches inside a rolling KO horizon (see <see cref="MatchProviderSyncOptions"/>).</summary>
internal sealed class MatchFeedSyncHostedService(
    IServiceScopeFactory scopes,
    IOptions<MatchProviderSyncOptions> opts,
    ILogger<MatchFeedSyncHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var o = opts.Value;
        if (!o.Enabled)
        {
            logger.LogInformation("{Service}: disabled ({Section}:Enabled=false).",
                nameof(MatchFeedSyncHostedService), MatchProviderSyncOptions.SectionName);
            return;
        }

        logger.LogInformation("{Service}: tick={Tick}s warm={Warm}m horizon+{Hor}h cap={Cap}.",
            nameof(MatchFeedSyncHostedService), o.TickSeconds, o.WarmWindowBeforeKickoffMinutes,
            o.PollHorizonHoursAfterKickoff, o.DailyRequestBudget?.ToString() ?? "none");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var sp = scope.ServiceProvider;
                var db = sp.GetRequiredService<BigBallDbContext>();
                var cfg = sp.GetRequiredService<IOptions<MatchProviderSyncOptions>>().Value;
                var corr = sp.GetRequiredService<MatchScheduleCorrelationService>();
                var sports = sp.GetRequiredService<ISportsDataSource>();
                var budget = sp.GetRequiredService<IProviderDailyApiBudget>();

                await RunTickAsync(db, cfg, corr, sports, budget, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Match feed sync tick faulted.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(Math.Clamp(opts.Value.TickSeconds, 15, 7200)),
                stoppingToken).ConfigureAwait(false);
        }
    }

    private static async Task RunTickAsync(
        BigBallDbContext db,
        MatchProviderSyncOptions o,
        MatchScheduleCorrelationService corr,
        ISportsDataSource sports,
        IProviderDailyApiBudget budget,
        CancellationToken ct)
    {
        var utc = DateTime.UtcNow;
        var candidates = await db.Matches.AsNoTracking()
            .Where(m => m.ExternalKey != null
                        && m.KickoffUtc >= utc.AddDays(-90)
                        && m.KickoffUtc <= utc.AddDays(365))
            .Select(x => x.Id)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        foreach (var id in candidates)
        {
            if (ct.IsCancellationRequested)
                return;

            var used = await budget.GetConsumedTodayUtcAsync(db, ct).ConfigureAwait(false);
            if (o.DailyRequestBudget is { } cap && used + 4 > cap)
                continue;

            var match = await db.Matches
                .FirstOrDefaultAsync(e => e.Id == id, ct)
                .ConfigureAwait(false);

            if (match is null)
                continue;

            var kick = DateTime.SpecifyKind(match.KickoffUtc, DateTimeKind.Utc);

            if (!MatchPollingIntervals.IsDue(utc,
                    kick,
                    match.LastLifecyclePhase,
                    match.ProviderLastSyncedUtc,
                    o))
                continue;

            var telemetry = new SportsMatchFetchTelemetry();

            if (!await corr.TryHydrateSportsApiIdAsync(match, telemetry, ct).ConfigureAwait(false)
                || string.IsNullOrEmpty(match.ProviderExternalMatchId))
                continue;

            await db.SaveChangesAsync(ct).ConfigureAwait(false);

            used = await budget.GetConsumedTodayUtcAsync(db, ct).ConfigureAwait(false);
            var planned = telemetry.HttpGetCount + 4;
            if (o.DailyRequestBudget is { } cap2 && used + planned > cap2)
                continue;

            var snap = await sports
                .GetMatchByExternalIdAsync(match.ProviderExternalMatchId!, telemetry, ct)
                .ConfigureAwait(false);

            await budget.AddConsumptionAsync(db, telemetry.HttpGetCount, ct).ConfigureAwait(false);

            if (snap is null)
                continue;

            await SportsMatchFeedSyncApplier
                .ApplyAsync(db, match, snap, utc, utc, ct)
                .ConfigureAwait(false);
            await db.SaveChangesAsync(ct).ConfigureAwait(false);
        }
    }
}
