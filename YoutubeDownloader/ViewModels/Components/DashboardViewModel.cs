using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gress;
using Gress.Completable;
using PowerKit;
using PowerKit.Extensions;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Core.Resolving;
using YoutubeDownloader.Core.Tagging;
using YoutubeDownloader.Core.Youtube;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Localization;
using YoutubeDownloader.Services;
using YoutubeExplode.Exceptions;

namespace YoutubeDownloader.ViewModels.Components;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly ViewModelManager _viewModelManager;
    private readonly SnackbarManager _snackbarManager;
    private readonly DialogManager _dialogManager;
    private readonly SettingsService _settingsService;
    private readonly DownloadStorageService _downloadStorageService;
    private readonly DownloadServiceManager _downloadServiceManager;

    private readonly IDisposable _eventSubscription;
    private readonly ResizableSemaphore _downloadSemaphore = new();
    private readonly AutoResetProgressMuxer _progressMuxer;

    private bool _isInitialized;

    public DashboardViewModel(
        ViewModelManager viewModelManager,
        SnackbarManager snackbarManager,
        DialogManager dialogManager,
        LocalizationManager localizationManager,
        SettingsService settingsService,
        DownloadStorageService downloadStorageService,
        DownloadServiceManager downloadServiceManager
    )
    {
        _viewModelManager = viewModelManager;
        _snackbarManager = snackbarManager;
        _dialogManager = dialogManager;
        LocalizationManager = localizationManager;
        _settingsService = settingsService;
        _downloadStorageService = downloadStorageService;
        _downloadServiceManager = downloadServiceManager;

        _progressMuxer = Progress.CreateMuxer().WithAutoReset();

        _eventSubscription = Disposable.Merge(
            _settingsService.WatchProperty(
                o => o.ParallelLimit,
                v => _downloadSemaphore.MaxCount = Math.Clamp(v, 1, 10),
                true
            ),
            Progress.WatchProperty(
                o => o.Current,
                v =>
                {
                    OnPropertyChanged(nameof(IsProgressIndeterminate));
                    _downloadServiceManager.ReportProgress(v.Fraction);
                }
            )
        );

        SharedQueryHub.QueryReceived += SharedQueryHub_OnQueryReceived;
    }

    public LocalizationManager LocalizationManager { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProgressIndeterminate))]
    [NotifyCanExecuteChangedFor(nameof(ProcessQueryCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowAuthSetupCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowSettingsCommand))]
    public partial bool IsBusy { get; set; }

    public ProgressContainer<Percentage> Progress { get; } = new();

    public bool IsProgressIndeterminate => IsBusy && Progress.Current.Fraction is <= 0 or >= 1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProcessQueryCommand))]
    public partial string? Query { get; set; }

    public ObservableCollection<DownloadViewModel> Downloads { get; } = [];

    private async Task EnsureFFmpegAsync()
    {
        // FFmpeg is bundled with the app, so it can only be missing if the installation is corrupted
        if (FFmpeg.IsAvailable())
            return;

        await _dialogManager.ShowDialogAsync(
            _viewModelManager.GetMessageBoxViewModel(
                LocalizationManager.FFmpegMissingTitle,
                string.Format(LocalizationManager.FFmpegMissingMessage, Program.Name)
            )
        );
    }

    public override async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

        await EnsureFFmpegAsync();

        // Process the text shared to the app before it was initialized
        ProcessSharedQuery();
    }

    private void SharedQueryHub_OnQueryReceived(object? sender, EventArgs args) =>
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_isInitialized)
                ProcessSharedQuery();
        });

    private void ProcessSharedQuery()
    {
        // Don't interrupt an ongoing operation, the shared query will be processed later
        if (IsBusy)
            return;

        if (SharedQueryHub.TryConsume() is not { } query)
            return;

        Query = query;
        ProcessQueryCommand.ExecuteIfCan(null);
    }

    private bool CanShowAuthSetup() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanShowAuthSetup))]
    private async Task ShowAuthSetupAsync()
    {
        await _dialogManager.ShowDialogAsync(_viewModelManager.GetAuthSetupViewModel());
        _settingsService.Save();
    }

    private bool CanShowSettings() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanShowSettings))]
    private async Task ShowSettingsAsync()
    {
        await _dialogManager.ShowDialogAsync(_viewModelManager.GetSettingsViewModel());
        _settingsService.Save();
    }

    private void UpdateDownloadService() =>
        _downloadServiceManager.Update(
            Downloads.Count(d => d.Status is DownloadStatus.Enqueued or DownloadStatus.Started)
        );

    private string GetErrorMessage(Exception ex) =>
        ex switch
        {
            // Explain how to resolve the error, since YouTube's message is not actionable in the app
            SignInRequiredException =>
                LocalizationManager.SignInRequiredMessage
                    + Environment.NewLine
                    + Environment.NewLine
                    + ex.Message,
            // Short error message for YouTube-related errors, full for others
            YoutubeExplodeException => ex.Message,
            _ => ex.ToString(),
        };

    private async void EnqueueDownload(DownloadViewModel download, int position = 0)
    {
        Downloads.Insert(position, download);
        UpdateDownloadService();

        var progress = _progressMuxer.CreateInput();

        // Files are downloaded to the app's cache first, and then saved to the shared storage.
        // Short file names are used to avoid exceeding file name length limits with temporary files.
        var workDirPath = Path.Combine(
            FileSystem.Current.CacheDirectory,
            "downloads",
            Guid.NewGuid().ToString("N")
        );

        try
        {
            using var downloader = new VideoDownloader(_settingsService.LastAuthCookies);
            var tagInjector = new MediaTagInjector();

            using var access = await _downloadSemaphore.AcquireAsync(download.CancellationToken);

            download.Status = DownloadStatus.Started;

            var downloadOption =
                download.DownloadOption
                ?? await downloader.GetBestDownloadOptionAsync(
                    download.Video!.Id,
                    download.DownloadPreference!,
                    _settingsService.ShouldInjectLanguageSpecificAudioStreams,
                    download.CancellationToken
                );

            Directory.CreateDirectory(workDirPath);
            var tempFilePath = Path.Combine(workDirPath, "video." + downloadOption.Container.Name);

            await downloader.DownloadVideoAsync(
                tempFilePath,
                download.Video!,
                downloadOption,
                _settingsService.ShouldInjectSubtitles,
                FFmpeg.CliFilePath,
                download.Progress.Merge(progress),
                download.CancellationToken
            );

            if (_settingsService.ShouldInjectTags)
            {
                try
                {
                    await tagInjector.InjectTagsAsync(
                        tempFilePath,
                        download.Video!,
                        download.CancellationToken
                    );
                }
                catch
                {
                    // Media tagging is not critical
                }
            }

            var fileName =
                !string.IsNullOrWhiteSpace(download.FileName)
                    ? Path.ChangeExtension(download.FileName, downloadOption.Container.Name)
                    : FileNameTemplate.Apply(
                        _settingsService.FileNameTemplate,
                        download.Video!,
                        downloadOption.Container
                    );

            var savedFile = await _downloadStorageService.SaveAsync(
                tempFilePath,
                fileName,
                download.CancellationToken
            );

            download.SavedFile = savedFile;
            download.FileName = savedFile.Name;
            download.Status = DownloadStatus.Completed;
        }
        catch (Exception ex)
        {
            download.Status =
                ex is OperationCanceledException ? DownloadStatus.Canceled : DownloadStatus.Failed;

            download.ErrorMessage = GetErrorMessage(ex);
        }
        finally
        {
            try
            {
                // Delete the temporary files
                if (Directory.Exists(workDirPath))
                    Directory.Delete(workDirPath, true);
            }
            catch
            {
                // Ignore
            }

            progress.ReportCompletion();
            download.Dispose();
            UpdateDownloadService();
        }
    }

    private bool CanProcessQuery() => !IsBusy && !string.IsNullOrWhiteSpace(Query);

    [RelayCommand(CanExecute = nameof(CanProcessQuery))]
    private async Task ProcessQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(Query))
            return;

        IsBusy = true;

        // Small weight so as to not offset any existing download operations
        var progress = _progressMuxer.CreateInput(0.01);

        try
        {
            using var resolver = new QueryResolver(_settingsService.LastAuthCookies);

            // Split queries by newlines
            var queries = Query.Split(
                '\n',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );

            // Process individual queries
            var queryResults = new List<QueryResult>();
            foreach (var (i, query) in queries.Index())
            {
                try
                {
                    queryResults.Add(await resolver.ResolveAsync(query));
                }
                // If it's not the only query in the list, don't interrupt the process
                // and report the error via an async notification instead of a sync dialog.
                // https://github.com/Tyrrrz/YoutubeDownloader/issues/563
                catch (YoutubeExplodeException ex)
                    when (ex is VideoUnavailableException or PlaylistUnavailableException
                        && queries.Length > 1
                    )
                {
                    _snackbarManager.Notify(GetErrorMessage(ex));
                }

                progress.Report(Percentage.FromFraction((i + 1.0) / queries.Length));
            }

            // Aggregate results
            var queryResult = QueryResult.Aggregate(queryResults);

            // Single video result
            if (queryResult.Videos.Count == 1)
            {
                var video = queryResult.Videos.Single();

                using var downloader = new VideoDownloader(_settingsService.LastAuthCookies);

                var downloadOptions = await downloader.GetDownloadOptionsAsync(
                    video.Id,
                    _settingsService.ShouldInjectLanguageSpecificAudioStreams
                );

                var download = await _dialogManager.ShowDialogAsync(
                    _viewModelManager.GetDownloadSingleSetupViewModel(video, downloadOptions)
                );

                if (download is null)
                    return;

                EnqueueDownload(download);

                Query = "";
            }
            // Multiple videos
            else if (queryResult.Videos.Count > 1)
            {
                var downloads = await _dialogManager.ShowDialogAsync(
                    _viewModelManager.GetDownloadMultipleSetupViewModel(
                        queryResult.Title,
                        queryResult.Videos,
                        // Pre-select videos if they come from a single query and not from search
                        queryResult.Kind
                            is not QueryResultKind.Search
                                and not QueryResultKind.Aggregate
                    )
                );

                if (downloads is null)
                    return;

                foreach (var download in downloads)
                    EnqueueDownload(download);

                Query = "";
            }
            // No videos found
            else
            {
                await _dialogManager.ShowDialogAsync(
                    _viewModelManager.GetMessageBoxViewModel(
                        LocalizationManager.NothingFoundTitle,
                        LocalizationManager.NothingFoundMessage
                    )
                );
            }
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelManager.GetMessageBoxViewModel(
                    LocalizationManager.ErrorTitle,
                    GetErrorMessage(ex)
                )
            );
        }
        finally
        {
            progress.ReportCompletion();
            IsBusy = false;

            // Process the text that was shared to the app while it was busy
            ProcessSharedQuery();
        }
    }

    private void RemoveDownload(DownloadViewModel download)
    {
        Downloads.Remove(download);
        download.CancelCommand.ExecuteIfCan(null);
        download.Dispose();
        UpdateDownloadService();
    }

    [RelayCommand]
    private void RemoveSuccessfulDownloads()
    {
        foreach (var download in Downloads.ToArray())
        {
            if (download.Status == DownloadStatus.Completed)
                RemoveDownload(download);
        }
    }

    [RelayCommand]
    private void RemoveInactiveDownloads()
    {
        foreach (var download in Downloads.ToArray())
        {
            if (
                download.Status
                is DownloadStatus.Completed
                    or DownloadStatus.Failed
                    or DownloadStatus.Canceled
            )
                RemoveDownload(download);
        }
    }

    [RelayCommand]
    private void RestartDownload(DownloadViewModel download)
    {
        var position = Math.Max(0, Downloads.IndexOf(download));
        RemoveDownload(download);

        var newDownload = download.DownloadOption is not null
            ? _viewModelManager.GetDownloadViewModel(
                download.Video!,
                download.DownloadOption,
                download.FileName!
            )
            : _viewModelManager.GetDownloadViewModel(
                download.Video!,
                download.DownloadPreference!,
                download.FileName!
            );

        EnqueueDownload(newDownload, position);
    }

    [RelayCommand]
    private void RestartFailedDownloads()
    {
        foreach (var download in Downloads.ToArray())
        {
            if (download.Status == DownloadStatus.Failed)
                RestartDownload(download);
        }
    }

    [RelayCommand]
    private void CancelAllDownloads()
    {
        foreach (var download in Downloads)
            download.CancelCommand.ExecuteIfCan(null);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SharedQueryHub.QueryReceived -= SharedQueryHub_OnQueryReceived;

            CancelAllDownloads();

            _eventSubscription.Dispose();
            _downloadSemaphore.Dispose();
        }

        base.Dispose(disposing);
    }
}
