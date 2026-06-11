using BigBall.Api.Auth;
using BigBall.Api.Configuration;
using BigBall.Api.Data;
using BigBall.Api.DependencyInjection;
using BigBall.Api.Endpoints;
using BigBall.Api.Sync;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Must run before any reads of values supplied only via user secrets (e.g. Supabase:ProjectUrl).
    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddUserSecrets<Program>();
    }

    builder.Logging.ClearProviders();
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration));

    builder.Services.Configure<SupabaseAuthOptions>(
        builder.Configuration.GetSection(SupabaseAuthOptions.SectionName));

    builder.Services.AddSportsData(builder.Configuration);
    var supabase = builder.Configuration.GetSection(SupabaseAuthOptions.SectionName).Get<SupabaseAuthOptions>()
                   ?? throw new InvalidOperationException("Supabase configuration missing.");

    builder.Services.AddDbContext<BigBallDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Supabase")));
    builder.Services.AddScoped<RankingService>();
    builder.Services.AddScoped<ProfileStatsService>();
    builder.Services.AddHttpClient<SupabasePasswordAuthClient>();

    var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>()
                      ?? new[] { "http://localhost:5180" };

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy => policy
            .WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
    });

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // Keep JWT claim types as issued by Supabase ("email", "sub", …) instead of mapping to long SOAP URIs.
            options.MapInboundClaims = false;
            options.Authority = supabase.AuthIssuer;
            options.MetadataAddress = $"{supabase.AuthIssuer}/.well-known/openid-configuration";
            options.TokenValidationParameters.ValidAudience = supabase.JwtAudience;
            options.TokenValidationParameters.ValidateAudience = true;
            options.TokenValidationParameters.ValidateIssuer = true;
            options.TokenValidationParameters.ValidateLifetime = true;
            options.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30);
        });
    builder.Services.AddAuthorization();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.Configure<MatchProviderSyncOptions>(
        builder.Configuration.GetSection(MatchProviderSyncOptions.SectionName));
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<IProviderDailyApiBudget, ProviderDailyApiBudgetService>();
    builder.Services.AddScoped<MatchScheduleCorrelationService>();
    builder.Services.AddHostedService<MatchFeedSyncHostedService>();

    var app = builder.Build();

    var startupLogger = app.Services.GetRequiredService<ILoggerFactory>()
        .CreateLogger("BigBall.Api.Startup");
    startupLogger.LogInformation(
        "BigBall API is starting. Environment: {Environment}, ContentRoot: {ContentRoot}.",
        app.Environment.EnvironmentName,
        app.Environment.ContentRootPath);

    using (var scope = app.Services.CreateScope())
    {
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<BigBallDbContext>();
        await db.Database.MigrateAsync();

        var config = sp.GetRequiredService<IConfiguration>();
        var env = sp.GetRequiredService<IWebHostEnvironment>();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("BigBall.Api.Fixtures");
        await HostCitiesSeeder.SeedAsync(db, env, logger);
        if (config.GetValue("Fixtures:ImportWorldCup2026", false))
        {
            // Single-arg StartsWith is translatable; the StringComparison overload is not.
            var hasImported = await db.Matches.AnyAsync(
                m => m.ExternalKey != null && m.ExternalKey.StartsWith("wc2026-"));
            if (!hasImported)
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Data", "worldcup-2026.json");
                if (!File.Exists(path))
                {
                    path = Path.Combine(env.ContentRootPath, "Data", "worldcup-2026.json");
                }

                if (File.Exists(path))
                {
                    await WorldCup2026FixtureImporter.ImportAsync(db, path);
                    logger.LogInformation("World Cup 2026 fixtures imported from {Path}", path);
                }
                else
                {
                    logger.LogWarning("Fixtures:ImportWorldCup2026 is true but worldcup-2026.json was not found.");
                }
            }
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow }))
       .AllowAnonymous()
       .WithTags("Health");

    app.MapAuthEndpoints();
    app.MapPoolsEndpoints();
    app.MapMatchesEndpoints();
    app.MapPredictionsEndpoints();
    app.MapTeamsEndpoints();

    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
