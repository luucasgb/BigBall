using BigBall.Client.Core.Abstractions;
using BigBall.Client.Core.ViewModels;
using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Tests;

public class PredictViewModelTests
{
    [Fact]
    public async Task LoadAsync_Unlocked_EnablesSave_AndBuildsPreviewWhenReferenceAvailable()
    {
        var now = new DateTime(2026, 06, 10, 14, 00, 00, DateTimeKind.Utc);
        // Future kickoff (unlocked) + reference scores (synthetic, to exercise preview path).
        var matches = new FakeMatchesApi(BuildMatchWithReference(
            kickoff: now.AddMinutes(60),
            refHome: 2, refAway: 1,
            seedPred: new PredictionDto(_matchId, 2, 1, null, now)));
        var vm = NewVm(matches, new FakePredictionsApi(), now);

        vm.PoolId = Guid.NewGuid();
        vm.MatchId = matches.MatchId;

        await vm.LoadCommand.ExecuteAsync(null);

        Assert.False(vm.IsLocked);
        Assert.True(vm.SaveCommand.CanExecute(null));
        Assert.NotNull(vm.Preview);
        Assert.Equal(20, vm.Preview!.Tier); // 2×1 vs 2×1 exact
    }

    [Fact]
    public async Task LoadAsync_AfterLockWindow_DisablesSave()
    {
        var now = new DateTime(2026, 06, 10, 14, 00, 00, DateTimeKind.Utc);
        var kickoff = now; // lock at kickoff → already past start for prediction window
        var matches = new FakeMatchesApi(BuildScheduledMatch(kickoff));
        var vm = NewVm(matches, new FakePredictionsApi(), now);

        vm.PoolId = Guid.NewGuid();
        vm.MatchId = matches.MatchId;

        await vm.LoadCommand.ExecuteAsync(null);

        Assert.True(vm.IsLocked);
        Assert.False(vm.SaveCommand.CanExecute(null));
    }

    [Fact]
    public async Task SaveAsync_Propagates409_SetsErrorAndReloads()
    {
        var now = new DateTime(2026, 06, 10, 14, 00, 00, DateTimeKind.Utc);
        var matches = new FakeMatchesApi(BuildScheduledMatch(now.AddMinutes(10)));
        var preds = new FakePredictionsApi { FailWithLocked = true };
        var vm = NewVm(matches, preds, now);

        vm.PoolId = Guid.NewGuid();
        vm.MatchId = matches.MatchId;
        await vm.LoadCommand.ExecuteAsync(null);

        await vm.SaveCommand.ExecuteAsync(null);

        Assert.False(string.IsNullOrWhiteSpace(vm.ErrorMessage));
        Assert.True(matches.CallCount >= 2); // initial load + reload after 409
    }

    private static PredictViewModel NewVm(FakeMatchesApi matches, FakePredictionsApi preds, DateTime now)
    {
        return new PredictViewModel(matches, preds, new FakeNavigator(), () => now);
    }

    private Guid _matchId = Guid.NewGuid();

    private MatchDetailDto BuildMatchWithReference(DateTime kickoff, int refHome, int refAway, PredictionDto? seedPred) => new(
        Id: _matchId,
        Phase: "Groups",
        GroupLabel: "Grupo A",
        HomeCode: "ARG",
        AwayCode: "CAN",
        KickoffUtc: kickoff,
        LockUtc: kickoff,
        Venue: null,
        HostCity: null,
        Status: "Scheduled",
        ReferenceHome: refHome,
        ReferenceAway: refAway,
        WentToPenalties: false,
        PenaltyWinnerCode: null,
        MyPrediction: seedPred);

    private MatchDetailDto BuildScheduledMatch(DateTime kickoff) => new(
        Id: _matchId,
        Phase: "Groups",
        GroupLabel: "Grupo A",
        HomeCode: "ARG",
        AwayCode: "MEX",
        KickoffUtc: kickoff,
        LockUtc: kickoff,
        Venue: null,
        HostCity: null,
        Status: "Scheduled",
        ReferenceHome: null,
        ReferenceAway: null,
        WentToPenalties: false,
        PenaltyWinnerCode: null,
        MyPrediction: null);

    private sealed class FakeMatchesApi : IMatchesApi
    {
        private MatchDetailDto _match;
        public Guid MatchId => _match.Id;
        public int CallCount { get; private set; }

        public FakeMatchesApi(MatchDetailDto match) => _match = match;

        public Task<IReadOnlyList<MatchCalendarRowDto>> GetMatchesInRangeAsync(
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<MatchCalendarRowDto>>([]);

        public Task<MatchDetailDto> GetMatchAsync(Guid matchId, Guid poolId, CancellationToken ct = default)
        {
            CallCount++;
            return Task.FromResult(_match);
        }

        public Task<IReadOnlyList<PoolPredictionDto>> GetMyPoolPredictionsAsync(Guid matchId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<PoolPredictionDto>>([]);
    }

    private sealed class FakePredictionsApi : IPredictionsApi
    {
        public bool FailWithLocked { get; set; }

        public Task<PredictionDto> UpsertAsync(Guid poolId, Guid matchId, UpsertPredictionRequest request, CancellationToken ct = default)
        {
            if (FailWithLocked) throw new PredictionLockedException("Palpite bloqueado.");
            return Task.FromResult(new PredictionDto(matchId, request.Home, request.Away, request.PenaltyWinnerCode, DateTime.UtcNow));
        }
    }

    private sealed class FakeNavigator : IAppNavigator
    {
        public void NavigateTo(string route, bool replace = false) { }
        public void NavigateToRoot() { }
    }
}
