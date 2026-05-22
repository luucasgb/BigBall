using BigBall.Api.Configuration;
using BigBall.Api.Integrations.SportsApiPro;
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
            provider = SportsDataProviderNames.SportsApiPro;

        switch (provider)
        {
            case SportsDataProviderNames.SportsApiPro:
                services.Configure<SportsApiProOptions>(
                    configuration.GetSection(SportsApiProOptions.SectionName));

                services.AddTransient<SportsApiProApiKeyHandler>();

                services.AddHttpClient<SportsApiProSportsDataSource>((sp, client) =>
                    {
                        var opts = sp.GetRequiredService<IOptions<SportsApiProOptions>>().Value;
                        var baseUrl = opts.BaseUrl.Trim();
                        if (string.IsNullOrEmpty(baseUrl))
                            throw new InvalidOperationException("SportsApiPro:BaseUrl is required.");

                        client.BaseAddress = new Uri(baseUrl.EndsWith("/", StringComparison.Ordinal) ? baseUrl : baseUrl + "/");
                        client.Timeout = TimeSpan.FromSeconds(30);
                    })
                    .AddHttpMessageHandler<SportsApiProApiKeyHandler>()
                    .AddPolicyHandler(BuildResiliencePolicy());

                services.AddTransient<ISportsDataSource>(sp =>
                    sp.GetRequiredService<SportsApiProSportsDataSource>());

                services.AddHttpClient<SportsApiProWorldCup2026ProbeService>((sp, client) =>
                    {
                        var opts = sp.GetRequiredService<IOptions<SportsApiProOptions>>().Value;
                        var baseUrl = opts.BaseUrl.Trim();
                        if (string.IsNullOrEmpty(baseUrl))
                            throw new InvalidOperationException("SportsApiPro:BaseUrl is required.");

                        client.BaseAddress =
                            new Uri(baseUrl.EndsWith("/", StringComparison.Ordinal) ? baseUrl : baseUrl + "/");
                        // Many round fetches per click — allow headroom vs default 30s.
                        client.Timeout = TimeSpan.FromSeconds(120);
                    })
                    .AddHttpMessageHandler<SportsApiProApiKeyHandler>()
                    .AddPolicyHandler(BuildResiliencePolicy());
                break;

            default:
                throw new InvalidOperationException(
                    $"Unknown SportsData:Provider '{provider}'. Supported: {SportsDataProviderNames.SportsApiPro}.");
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
