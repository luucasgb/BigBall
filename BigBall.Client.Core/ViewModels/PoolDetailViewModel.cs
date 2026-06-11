using System.Collections.ObjectModel;
using BigBall.Client.Core.Abstractions;
using BigBall.Client.Core.ViewModels.Base;
using BigBall.Shared.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BigBall.Client.Core.ViewModels;

public enum PoolTab { Ranking, Matches, Prize, Members }

public partial class PoolDetailViewModel : ViewModelBase
{
    private readonly IPoolsApi _pools;
    private readonly IAppNavigator _navigator;

    public PoolDetailViewModel(IPoolsApi pools, IAppNavigator navigator)
    {
        _pools = pools;
        _navigator = navigator;
    }

    [ObservableProperty]
    private Guid _poolId;

    [ObservableProperty]
    private PoolDetailDto? _pool;

    public ObservableCollection<RankingRowDto> Ranking { get; } = new();

    public ObservableCollection<PoolMatchRowDto> Matches { get; } = new();

    public RankingRowDto? MyRow => Ranking.FirstOrDefault(r => r.IsMe);

    public bool HasTieWithMe => MyRow?.TieGroupId is not null
        && Ranking.Count(r => r.TieGroupId == MyRow.TieGroupId) > 1;

    [ObservableProperty]
    private PoolTab _selectedTab = PoolTab.Ranking;

    [RelayCommand]
    public Task LoadAsync(CancellationToken ct) => RunAsync(async token =>
    {
        if (PoolId == Guid.Empty) return;
        var detail = await _pools.GetPoolAsync(PoolId, token).ConfigureAwait(false);
        Pool = detail;
        Ranking.Clear();
        foreach (var row in detail.Ranking) Ranking.Add(row);
        OnPropertyChanged(nameof(MyRow));
        OnPropertyChanged(nameof(HasTieWithMe));

        var matches = await _pools.GetPoolMatchesAsync(PoolId, token).ConfigureAwait(false);
        Matches.Clear();
        foreach (var match in matches) Matches.Add(match);
    }, ct);

    [RelayCommand]
    private void GoBack() => _navigator.NavigateTo("/", replace: true);

    [RelayCommand]
    private void SelectTab(PoolTab tab) => SelectedTab = tab;
}
