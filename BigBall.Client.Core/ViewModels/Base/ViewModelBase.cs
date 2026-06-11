using CommunityToolkit.Mvvm.ComponentModel;

namespace BigBall.Client.Core.ViewModels.Base;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    protected async Task RunAsync(Func<CancellationToken, Task> work, CancellationToken ct = default)
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await work(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
