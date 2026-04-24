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

    partial void OnFeaturedPoolChanged(MyPoolDto? value) => OnPropertyChanged(nameof(FeaturedNextMatch));

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
    private void OpenFeaturedPrediction()
    {
        if (FeaturedPool is null || FeaturedPool.NextMatch is null) return;
        _navigator.NavigateTo($"/pools/{FeaturedPool.Id}/predict/{FeaturedPool.NextMatch.Id}");
    }
}
