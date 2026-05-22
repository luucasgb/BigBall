using BigBall.Client.Core.Abstractions;
using BigBall.Client.Core.Http;
using BigBall.Client.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BigBall.Client.Core.DependencyInjection;

public static class ClientCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers API clients (Auth/Pools/Matches/Predictions/Teams) + the 4 ViewModels.
    /// Consumers must additionally register <see cref="ITokenStore"/>,
    /// <see cref="IUserProfileStore"/>, and <see cref="IAppNavigator"/> implementations for their platform.
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

        // Teams catalog is anonymous and cacheable — no auth handler.
        services.AddHttpClient<ITeamsApi, TeamsApiClient>(c => c.BaseAddress = apiBaseAddress);

        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<PoolDetailViewModel>();
        services.AddTransient<PredictViewModel>();

        return services;
    }
}
