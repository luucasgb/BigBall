using System.Collections.ObjectModel;
using BigBall.Client.Core.Abstractions;
using BigBall.Client.Core.ViewModels.Base;
using BigBall.Shared.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BigBall.Client.Core.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly IPoolsApi _pools;
    private readonly IAppNavigator _navigator;

    public HomeViewModel(IPoolsApi pools, IAppNavigator navigator)
    {
        _pools = pools;
        _navigator = navigator;
    }

    public ObservableCollection<MyPoolDto> Pools { get; } = new();

    [ObservableProperty]
    private MyPoolDto? _featuredPool;

    public NextMatchDto? FeaturedNextMatch => FeaturedPool?.NextMatch;

    public IReadOnlyList<FeaturedPoolPrediction> FeaturedPoolPredictions =>
        FeaturedNextMatch is not { } match
            ? Array.Empty<FeaturedPoolPrediction>()
            : Pools.Where(p => p.NextMatch?.Id == match.Id)
                   .Select(p => new FeaturedPoolPrediction(p.Id, p.Name, p.NextMatch!.MyPrediction))
                   .ToList();

    partial void OnFeaturedPoolChanged(MyPoolDto? value)
    {
        OnPropertyChanged(nameof(FeaturedNextMatch));
        OnPropertyChanged(nameof(FeaturedPoolPredictions));
    }

    [RelayCommand]
    public Task LoadAsync(CancellationToken ct) => RunAsync(async token =>
    {
        var result = await _pools.GetMyPoolsAsync(token).ConfigureAwait(false);
        Pools.Clear();
        foreach (var p in result) Pools.Add(p);

        FeaturedPool = Pools
            .Where(p => p.NextMatch is not null)
            .OrderBy(p => p.NextMatch!.KickoffUtc)
            .FirstOrDefault() ?? Pools.FirstOrDefault();
    }, ct);

    [RelayCommand]
    private void OpenPool(MyPoolDto pool) => _navigator.NavigateTo($"/pools/{pool.Id}");

    [RelayCommand]
    private void OpenPrediction(Guid poolId)
    {
        if (FeaturedNextMatch is not { } match) return;
        // Palpitar acontece no calendário (a página dedicada de palpite foi removida); abre o jogo e o bolão já selecionados.
        _navigator.NavigateTo($"/calendar?match={match.Id}&pool={poolId}");
    }
}

public sealed record FeaturedPoolPrediction(Guid PoolId, string PoolName, ScoreDto? Prediction);
