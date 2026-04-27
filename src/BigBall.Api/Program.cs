using BigBall.Api.Auth;
using BigBall.Api.Data;
using BigBall.Api.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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
    var supabase = builder.Configuration.GetSection(SupabaseAuthOptions.SectionName).Get<SupabaseAuthOptions>()
                   ?? throw new InvalidOperationException("Supabase configuration missing.");

    builder.Services.AddDbContext<BigBallDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Supabase")));
    builder.Services.AddScoped<RankingService>();
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

    var app = builder.Build();

    var startupLogger = app.Services.GetRequiredService<ILoggerFactory>()
        .CreateLogger("BigBall.Api.Startup");
    startupLogger.LogInformation(
        "BigBall API is starting. Environment: {Environment}, ContentRoot: {ContentRoot}.",
        app.Environment.EnvironmentName,
        app.Environment.ContentRootPath);

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<BigBallDbContext>();
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db);
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

    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
