using BigBall.Client.Core.Abstractions;
using BigBall.Client.Core.Scoring;
using BigBall.Client.Core.ViewModels.Base;
using BigBall.Shared.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BigBall.Client.Core.ViewModels;

public partial class PredictViewModel : ViewModelBase, IDisposable
{
    private const int MinScore = 0;
    private const int MaxScore = 20;

    private readonly IMatchesApi _matches;
    private readonly IPredictionsApi _predictions;
    private readonly IAppNavigator _navigator;
    private readonly Func<DateTime> _nowProvider;

    private PeriodicTimer? _tickTimer;
    private CancellationTokenSource? _timerCts;

    public PredictViewModel(
        IMatchesApi matches,
        IPredictionsApi predictions,
        IAppNavigator navigator,
        Func<DateTime>? nowProvider = null)
    {
        _matches = matches;
        _predictions = predictions;
        _navigator = navigator;
        _nowProvider = nowProvider ?? (() => DateTime.UtcNow);
    }

    [ObservableProperty]
    private Guid _poolId;

    [ObservableProperty]
    private Guid _matchId;

    [ObservableProperty]
    private MatchDetailDto? _match;

    [ObservableProperty]
    private int _homePrediction;

    [ObservableProperty]
    private int _awayPrediction;

    [ObservableProperty]
    private string? _penaltyWinnerCode;

    [ObservableProperty]
    private int _secondsUntilLock;

    [ObservableProperty]
    private bool _isLocked;

    [ObservableProperty]
    private bool _isSaved;

    public ScoringPreview? Preview { get; private set; }

    public bool IsKnockout => Match is { } m && m.Phase != nameof(BigBall.Domain.Enums.MatchPhase.Groups);

    [RelayCommand]
    public Task LoadAsync(CancellationToken ct) => RunAsync(async token =>
    {
        await LoadCoreAsync(token).ConfigureAwait(false);
        StartTicking();
    }, ct);

    private async Task LoadCoreAsync(CancellationToken token)
    {
        if (MatchId == Guid.Empty || PoolId == Guid.Empty) return;
        var detail = await _matches.GetMatchAsync(MatchId, PoolId, token).ConfigureAwait(false);
        Match = detail;
        HomePrediction = detail.MyPrediction?.Home ?? 0;
        AwayPrediction = detail.MyPrediction?.Away ?? 0;
        PenaltyWinnerCode = detail.MyPrediction?.PenaltyWinnerCode;
        IsSaved = detail.MyPrediction is not null;
        UpdateLockState();
        UpdatePreview();
    }

    [RelayCommand(CanExecute = nameof(CanChangeScore))]
    private void IncrementHome() => HomePrediction = Math.Min(MaxScore, HomePrediction + 1);

    [RelayCommand(CanExecute = nameof(CanChangeScore))]
    private void DecrementHome() => HomePrediction = Math.Max(MinScore, HomePrediction - 1);

    [RelayCommand(CanExecute = nameof(CanChangeScore))]
    private void IncrementAway() => AwayPrediction = Math.Min(MaxScore, AwayPrediction + 1);

    [RelayCommand(CanExecute = nameof(CanChangeScore))]
    private void DecrementAway() => AwayPrediction = Math.Max(MinScore, AwayPrediction - 1);

    [RelayCommand(CanExecute = nameof(CanChangeScore))]
    private void PickPenaltyWinner(string teamCode) => PenaltyWinnerCode = teamCode;

    private bool CanChangeScore() => !IsLocked && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanSave))]
    public Task SaveAsync(CancellationToken ct) => RunAsync(async token =>
    {
        try
        {
            var req = new UpsertPredictionRequest(HomePrediction, AwayPrediction, PenaltyWinnerCode);
            var saved = await _predictions.UpsertAsync(PoolId, MatchId, req, token).ConfigureAwait(false);
            IsSaved = true;
            if (Match is not null)
            {
                Match = Match with { MyPrediction = saved };
            }
        }
        catch (PredictionLockedException ex)
        {
            ErrorMessage = ex.Message;
            // Reload match state inline — LoadAsync goes through RunAsync and would no-op
            // while we're still inside this SaveAsync RunAsync scope.
            await LoadCoreAsync(token).ConfigureAwait(false);
            OnPropertyChanged(nameof(CanSave));
        }
    }, ct);

    private bool CanSave() => !IsLocked && !IsBusy;

    [RelayCommand]
    private void GoBack() => _navigator.NavigateTo($"/pools/{PoolId}");

    partial void OnHomePredictionChanged(int value) => UpdatePreview();
    partial void OnAwayPredictionChanged(int value) => UpdatePreview();
    partial void OnPenaltyWinnerCodeChanged(string? value) => UpdatePreview();
    partial void OnIsLockedChanged(bool value) => NotifyEditCommandsCanExecuteChanged();

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(IsBusy))
        {
            NotifyEditCommandsCanExecuteChanged();
        }
    }

    private void NotifyEditCommandsCanExecuteChanged()
    {
        IncrementHomeCommand.NotifyCanExecuteChanged();
        DecrementHomeCommand.NotifyCanExecuteChanged();
        IncrementAwayCommand.NotifyCanExecuteChanged();
        DecrementAwayCommand.NotifyCanExecuteChanged();
        PickPenaltyWinnerCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
    }

    private void UpdatePreview()
    {
        Preview = Match is null
            ? null
            : ScoringPreview.TryCompute(
                HomePrediction, AwayPrediction,
                Match.ReferenceHome, Match.ReferenceAway,
                PenaltyWinnerCode, Match.PenaltyWinnerCode);
        OnPropertyChanged(nameof(Preview));
    }

    private void UpdateLockState()
    {
        if (Match is null)
        {
            SecondsUntilLock = 0;
            IsLocked = true;
            return;
        }
        var now = _nowProvider();
        var delta = Match.LockUtc - now;
        SecondsUntilLock = (int)Math.Max(0, Math.Ceiling(delta.TotalSeconds));
        IsLocked = delta <= TimeSpan.Zero;
    }

    private async void StartTicking()
    {
        StopTicking();
        if (IsLocked) return;

        _timerCts = new CancellationTokenSource();
        _tickTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            while (await _tickTimer.WaitForNextTickAsync(_timerCts.Token).ConfigureAwait(false))
            {
                UpdateLockState();
                if (IsLocked) break;
            }
        }
        catch (OperationCanceledException) { }
    }

    private void StopTicking()
    {
        _timerCts?.Cancel();
        _tickTimer?.Dispose();
        _timerCts?.Dispose();
        _tickTimer = null;
        _timerCts = null;
    }

    public void Dispose() => StopTicking();
}
