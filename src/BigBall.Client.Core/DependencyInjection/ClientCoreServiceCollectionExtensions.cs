using BigBall.Client.Core.Abstractions;
using BigBall.Client.Core.Http;
using BigBall.Client.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BigBall.Client.Core.DependencyInjection;

public static class ClientCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers API clients (Auth/Pools/Matches/Predictions) + the 4 ViewModels.
    /// Consumers must additionally register <see cref="ITokenStore"/> and
    /// <see cref="IAppNavigator"/> implementations for their platform.
    /// </summary>
    public static IServiceCollection AddBigBallClientCore(
        this IServiceCollection services,
        Uri apiBaseAddress)
    {
        services.AddTransient<AuthMessageHandler>();

        services.AddHttpClient<IAuthApi, AuthApiClient>(c => c.BaseAddress = apiBaseAddress)
                .AddHttpMessageHandler<AuthMessageHandler>();
        services.AddHttpClient<IPoolsApi, PoolsApiClient>(c => c.BaseAddress = apiBaseAddress)
                .AddHttpMessageHandler<AuthMessageHandler>();
        services.AddHttpClient<IMatchesApi, MatchesApiClient>(c => c.BaseAddress = apiBaseAddress)
                .AddHttpMessageHandler<AuthMessageHandler>();
        services.AddHttpClient<IPredictionsApi, PredictionsApiClient>(c => c.BaseAddress = apiBaseAddress)
                .AddHttpMessageHandler<AuthMessageHandler>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<PoolDetailViewModel>();
        services.AddTransient<PredictViewModel>();

        return services;
    }
}
