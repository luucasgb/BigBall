using BigBall.Api.Configuration;
using BigBall.Api.Integrations.FlashScore;
using BigBall.Domain.SportsData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace BigBall.Api.DependencyInjection;

/// <summary>Registers canonical <see cref="ISportsDataSource"/> and configured provider adapters.</summary>
public static class SportsDataServiceCollectionExtensions
{
    /// <exception cref="InvalidOperationException">Missing or unknown SportsData:Provider.</exception>
    public static IServiceCollection AddSportsData(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SportsDataOptions>(configuration.GetSection(SportsDataOptions.SectionName));

        var provider = configuration[$"{SportsDataOptions.SectionName}:Provider"]?.Trim();
        if (string.IsNullOrEmpty(provider))
            provider = SportsDataProviderNames.FlashScore;

        switch (provider)
        {
            case SportsDataProviderNames.FlashScore:
                services.Configure<FlashScoreOptions>(
                    configuration.GetSection(FlashScoreOptions.SectionName));

                services.AddTransient<FlashScoreApiKeyHandler>();

                services.AddHttpClient<FlashScoreSportsDataSource>((sp, client) =>
                    {
                        var opts = sp.GetRequiredService<IOptions<FlashScoreOptions>>().Value;
                        var baseUrl = opts.BaseUrl.Trim();
                        if (string.IsNullOrEmpty(baseUrl))
                            throw new InvalidOperationException("FlashScore:BaseUrl is required.");

                        client.BaseAddress = new Uri(baseUrl.EndsWith("/", StringComparison.Ordinal) ? baseUrl : baseUrl + "/");
                        client.Timeout = TimeSpan.FromSeconds(30);
                    })
                    .AddHttpMessageHandler<FlashScoreApiKeyHandler>()
                    .AddPolicyHandler(BuildResiliencePolicy());

                services.AddTransient<ISportsDataSource>(sp =>
                    sp.GetRequiredService<FlashScoreSportsDataSource>());

                services.AddHttpClient<FlashScoreTeamSearchService>((sp, client) =>
                    {
                        var opts = sp.GetRequiredService<IOptions<FlashScoreOptions>>().Value;
                        var baseUrl = opts.BaseUrl.Trim();
                        if (string.IsNullOrEmpty(baseUrl))
                            throw new InvalidOperationException("FlashScore:BaseUrl is required.");

                        client.BaseAddress = new Uri(baseUrl.EndsWith("/", StringComparison.Ordinal) ? baseUrl : baseUrl + "/");
                        client.Timeout = TimeSpan.FromSeconds(60);
                    })
                    .AddHttpMessageHandler<FlashScoreApiKeyHandler>()
                    .AddPolicyHandler(BuildResiliencePolicy());
                break;

            default:
                throw new InvalidOperationException(
                    $"Unknown SportsData:Provider '{provider}'. Supported: {SportsDataProviderNames.FlashScore}.");
        }

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> BuildResiliencePolicy()
    {
        var retry = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

        var circuitBreaker = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        return Policy.WrapAsync(retry, circuitBreaker);
    }
}
