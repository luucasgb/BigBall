using BigBall.Api.Auth;
using BigBall.Api.Data;
using BigBall.Api.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// STUB — substituir por Supabase JWT conforme TechSpec §4.3. Chave em appsettings NÃO é segredo de produção.
var jwtIssuer = new StubJwtIssuer(builder.Configuration);
builder.Services.AddSingleton(jwtIssuer);

builder.Services.AddSingleton<InMemoryStore>();
builder.Services.AddSingleton<RankingService>();

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
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer.Issuer,
            ValidAudience = jwtIssuer.Audience,
            IssuerSigningKey = jwtIssuer.SigningKey,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Populate in-memory seed data on startup.
SeedData.Populate(app.Services.GetRequiredService<InMemoryStore>());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
