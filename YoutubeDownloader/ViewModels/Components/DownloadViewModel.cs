using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gress;
using PowerKit.Extensions;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Localization;
using YoutubeDownloader.Services;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Components;

public partial class DownloadViewModel : ViewModelBase
{
    private readonly ViewModelManager _viewModelManager;
    private readonly DialogManager _dialogManager;
    private readonly SnackbarManager _snackbarManager;

    private readonly IDisposable _eventSubscription;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private bool _isDisposed;

    public DownloadViewModel(
        ViewModelManager viewModelManager,
        DialogManager dialogManager,
        SnackbarManager snackbarManager,
        LocalizationManager localizationManager
    )
    {
        _viewModelManager = viewModelManager;
        _dialogManager = dialogManager;
        _snackbarManager = snackbarManager;
        LocalizationManager = localizationManager;

        _eventSubscription = Progress.WatchProperty(
            o => o.Current,
            _ =>
            {
                OnPropertyChanged(nameof(IsProgressIndeterminate));
                OnPropertyChanged(nameof(StatusText));
            }
        );
    }

    public LocalizationManager LocalizationManager { get; }

    [ObservableProperty]
    public partial IVideo? Video { get; set; }

    [ObservableProperty]
    public partial VideoDownloadOption? DownloadOption { get; set; }

    [ObservableProperty]
    public partial VideoDownloadPreference? DownloadPreference { get; set; }

    // On Android, files are downloaded to a temporary location first and then saved
    // to the shared storage, so only the desired file name is known in advance.
    [ObservableProperty]
    public partial string? FileName { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShareFileCommand))]
    public partial SavedFile? SavedFile { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCanceledOrFailed))]
    [NotifyPropertyChangedFor(nameof(IsCancelable))]
    [NotifyPropertyChangedFor(nameof(IsStarted))]
    [NotifyPropertyChangedFor(nameof(IsCompleted))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShareFileCommand))]
    public partial DownloadStatus Status { get; set; } = DownloadStatus.Enqueued;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShowErrorMessageCommand))]
    public partial string? ErrorMessage { get; set; }

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public ProgressContainer<Percentage> Progress { get; } = new();

    public bool IsProgressIndeterminate => Progress.Current.Fraction is <= 0 or >= 1;

    public bool IsCanceledOrFailed => Status is DownloadStatus.Canceled or DownloadStatus.Failed;

    public bool IsCancelable => Status is DownloadStatus.Enqueued or DownloadStatus.Started;

    public bool IsStarted => Status == DownloadStatus.Started;

    public bool IsCompleted => Status == DownloadStatus.Completed;

    public string StatusText =>
        Status switch
        {
            DownloadStatus.Enqueued => LocalizationManager.DownloadStatusEnqueued,
            DownloadStatus.Started => IsProgressIndeterminate
                ? "…"
                : Progress.Current.ToString(),
            DownloadStatus.Completed => LocalizationManager.DownloadStatusCompleted,
            DownloadStatus.Canceled => LocalizationManager.DownloadStatusCanceled,
            DownloadStatus.Failed => LocalizationManager.DownloadStatusFailed,
            _ => string.Empty,
        };

    private bool CanCancel() => Status is DownloadStatus.Enqueued or DownloadStatus.Started;

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        if (_isDisposed)
            return;

        _cancellationTokenSource.Cancel();
    }

    private bool CanOpenFile() => Status == DownloadStatus.Completed && SavedFile is not null;

    [RelayCommand(CanExecute = nameof(CanOpenFile))]
    private async Task OpenFileAsync()
    {
        if (SavedFile is null)
            return;

        try
        {
            await DownloadStorageService.OpenAsync(SavedFile);
        }
        catch (global::Android.Content.ActivityNotFoundException)
        {
            _snackbarManager.Notify(LocalizationManager.NoAppToOpenFileMessage);
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelManager.GetMessageBoxViewModel(LocalizationManager.ErrorTitle, ex.Message)
            );
        }
    }

    [RelayCommand(CanExecute = nameof(CanOpenFile))]
    private async Task ShareFileAsync()
    {
        if (SavedFile is null)
            return;

        try
        {
            await DownloadStorageService.ShareAsync(SavedFile, Video?.Title);
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelManager.GetMessageBoxViewModel(LocalizationManager.ErrorTitle, ex.Message)
            );
        }
    }

    private bool CanShowErrorMessage() => !string.IsNullOrWhiteSpace(ErrorMessage);

    [RelayCommand(CanExecute = nameof(CanShowErrorMessage))]
    private async Task ShowErrorMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(ErrorMessage))
            return;

        var dialog = _viewModelManager.GetMessageBoxViewModel(
            LocalizationManager.ErrorTitle,
            ErrorMessage,
            LocalizationManager.CopyButton,
            LocalizationManager.CloseButton
        );

        if (await _dialogManager.ShowDialogAsync(dialog) == true)
            await CopyErrorMessageAsync();
    }

    [RelayCommand]
    private async Task CopyErrorMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(ErrorMessage))
            return;

        await Clipboard.Default.SetTextAsync(ErrorMessage);
    }

    protected override void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        _eventSubscription.Dispose();
        _cancellationTokenSource.Dispose();
    }
}
