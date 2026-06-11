using BigBall.Client.Core.Abstractions;
using BigBall.Client.Core.ViewModels;
using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Tests;

public class HomeViewModelTests
{
    private static readonly DateTime Kickoff = new(2026, 06, 11, 19, 00, 00, DateTimeKind.Utc);

    [Fact]
    public async Task LoadAsync_PoolsSharingFeaturedMatch_AllAppearWithTheirPredictions()
    {
        var matchId = Guid.NewGuid();
        var pools = new FakePoolsApi(
            BuildPool("Teste", matchId, Kickoff, new ScoreDto(2, 3)),
            BuildPool("Privado Teste", matchId, Kickoff, null));
        var vm = new HomeViewModel(pools, new FakeNavigator());

        await vm.LoadCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.FeaturedPoolPredictions.Count);
        var teste = Assert.Single(vm.FeaturedPoolPredictions, p => p.PoolName == "Teste");
        Assert.Equal(new ScoreDto(2, 3), teste.Prediction);
        var privado = Assert.Single(vm.FeaturedPoolPredictions, p => p.PoolName == "Privado Teste");
        Assert.Null(privado.Prediction);
    }

    [Fact]
    public async Task LoadAsync_PoolWithDifferentNextMatch_IsExcludedFromFeaturedPredictions()
    {
        var matchId = Guid.NewGuid();
        var pools = new FakePoolsApi(
            BuildPool("Teste", matchId, Kickoff, new ScoreDto(2, 3)),
            BuildPool("Outro", Guid.NewGuid(), Kickoff.AddDays(1), new ScoreDto(1, 0)));
        var vm = new HomeViewModel(pools, new FakeNavigator());

        await vm.LoadCommand.ExecuteAsync(null);

        var row = Assert.Single(vm.FeaturedPoolPredictions);
        Assert.Equal("Teste", row.PoolName);
    }

    [Fact]
    public async Task LoadAsync_NoPoolsOrNoNextMatch_YieldsEmptyPredictions()
    {
        var vmEmpty = new HomeViewModel(new FakePoolsApi(), new FakeNavigator());
        await vmEmpty.LoadCommand.ExecuteAsync(null);
        Assert.Empty(vmEmpty.FeaturedPoolPredictions);

        var vmNoMatch = new HomeViewModel(new FakePoolsApi(BuildPool("Teste", null, default, null)), new FakeNavigator());
        await vmNoMatch.LoadCommand.ExecuteAsync(null);
        Assert.Empty(vmNoMatch.FeaturedPoolPredictions);
    }

    [Fact]
    public async Task OpenPrediction_NavigatesToCalendarWithMatchAndPoolSelected()
    {
        var matchId = Guid.NewGuid();
        var pool = BuildPool("Teste", matchId, Kickoff, null);
        var navigator = new FakeNavigator();
        var vm = new HomeViewModel(new FakePoolsApi(pool), navigator);
        await vm.LoadCommand.ExecuteAsync(null);

        vm.OpenPredictionCommand.Execute(pool.Id);

        Assert.Equal($"/calendar?match={matchId}&pool={pool.Id}", navigator.LastRoute);
    }

    [Fact]
    public void OpenPrediction_WithoutFeaturedMatch_DoesNotNavigate()
    {
        var navigator = new FakeNavigator();
        var vm = new HomeViewModel(new FakePoolsApi(), navigator);

        vm.OpenPredictionCommand.Execute(Guid.NewGuid());

        Assert.Null(navigator.LastRoute);
    }

    private static MyPoolDto BuildPool(string name, Guid? matchId, DateTime kickoff, ScoreDto? prediction) => new(
        Id: Guid.NewGuid(),
        Name: name,
        MemberCount: 1,
        MyRank: 1,
        MyPoints: 0,
        Leader: new LeaderDto(Guid.NewGuid(), "Líder", 0),
        NextMatch: matchId is { } id
            ? new NextMatchDto(id, "MEX", "RSA", kickoff, "Group A", prediction)
            : null);

    private sealed class FakePoolsApi : IPoolsApi
    {
        private readonly IReadOnlyList<MyPoolDto> _pools;

        public FakePoolsApi(params MyPoolDto[] pools) => _pools = pools;

        public Task<IReadOnlyList<MyPoolDto>> GetMyPoolsAsync(CancellationToken ct = default) =>
            Task.FromResult(_pools);

        public Task<PoolDetailDto> GetPoolAsync(Guid poolId, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<PoolMatchRowDto>> GetPoolMatchesAsync(Guid poolId, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<CreatePoolResponse> CreatePoolAsync(CreatePoolRequest request, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<JoinPoolResponse> JoinPoolByInviteAsync(JoinPoolRequest request, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<PublicPoolDto>> GetPublicPoolsAsync(CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<JoinPoolResponse> JoinPublicPoolAsync(Guid poolId, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }

    private sealed class FakeNavigator : IAppNavigator
    {
        public string? LastRoute { get; private set; }
        public void NavigateTo(string route, bool replace = false) => LastRoute = route;
        public void NavigateToRoot() { }
    }
}
