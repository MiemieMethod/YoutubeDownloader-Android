using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Localization;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Components;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using Container = YoutubeExplode.Videos.Streams.Container;

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class DownloadMultipleSetupViewModel(
    ViewModelManager viewModelManager,
    SnackbarManager snackbarManager,
    LocalizationManager localizationManager,
    SettingsService settingsService,
    DownloadStorageService downloadStorageService
) : DialogViewModelBase<IReadOnlyList<DownloadViewModel>>
{
    public LocalizationManager LocalizationManager { get; } = localizationManager;

    [ObservableProperty]
    public partial string? Title { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<IVideo>? AvailableVideos { get; set; }

    public IReadOnlyList<SelectableVideoViewModel> Items { get; private set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedContainerIndex))]
    [NotifyPropertyChangedFor(nameof(IsVideoQualitySelectable))]
    public partial Container SelectedContainer { get; set; } = Container.Mp4;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedVideoQualityPreferenceIndex))]
    public partial VideoQualityPreference SelectedVideoQualityPreference { get; set; } =
        VideoQualityPreference.Highest;

    public ObservableCollection<IVideo> SelectedVideos { get; } = [];

    public IReadOnlyList<Container> AvailableContainers { get; } =
    [Container.Mp4, Container.WebM, Container.Mp3, new("ogg")];

    public IReadOnlyList<string> AvailableContainerNames =>
        AvailableContainers.Select(c => c.Name).ToArray();

    public int SelectedContainerIndex
    {
        get => AvailableContainers.ToList().IndexOf(SelectedContainer);
        set
        {
            if (value >= 0 && value < AvailableContainers.Count)
                SelectedContainer = AvailableContainers[value];
        }
    }

    public bool IsVideoQualitySelectable => !SelectedContainer.IsAudioOnly;

    public IReadOnlyList<VideoQualityPreference> AvailableVideoQualityPreferences { get; } =
        // Without .AsEnumerable(), the below line throws a compile-time error starting with .NET SDK v9.0.200
        Enum.GetValues<VideoQualityPreference>().AsEnumerable().Reverse().ToArray();

    public IReadOnlyList<string> AvailableVideoQualityPreferenceNames =>
        AvailableVideoQualityPreferences.Select(p => p.GetDisplayName()).ToArray();

    public int SelectedVideoQualityPreferenceIndex
    {
        get => AvailableVideoQualityPreferences.ToList().IndexOf(SelectedVideoQualityPreference);
        set
        {
            if (value >= 0 && value < AvailableVideoQualityPreferences.Count)
                SelectedVideoQualityPreference = AvailableVideoQualityPreferences[value];
        }
    }

    public bool AreAllVideosSelected => Items.Count > 0 && Items.All(i => i.IsSelected);

    public string ConfirmButtonText =>
        $"{LocalizationManager.DownloadButton} ({SelectedVideos.Count})";

    public void SetAvailableVideos(IReadOnlyList<IVideo> videos, bool preselectVideos)
    {
        AvailableVideos = videos;

        foreach (var item in Items)
            item.PropertyChanged -= Item_OnPropertyChanged;

        Items = videos.Select(v => new SelectableVideoViewModel(v, preselectVideos)).ToArray();

        foreach (var item in Items)
            item.PropertyChanged += Item_OnPropertyChanged;

        OnPropertyChanged(nameof(Items));
        SyncSelectedVideos();
    }

    private void Item_OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(SelectableVideoViewModel.IsSelected))
            SyncSelectedVideos();
    }

    // Keep the selected videos in the same order as they appear in the list
    private void SyncSelectedVideos()
    {
        SelectedVideos.Clear();
        foreach (var item in Items)
        {
            if (item.IsSelected)
                SelectedVideos.Add(item.Video);
        }

        OnPropertyChanged(nameof(AreAllVideosSelected));
        OnPropertyChanged(nameof(ConfirmButtonText));
        ConfirmCommand.NotifyCanExecuteChanged();
    }

    public override Task InitializeAsync()
    {
        SelectedContainer = settingsService.LastContainer;
        SelectedVideoQualityPreference = settingsService.LastVideoQualityPreference;

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task CopyTitleAsync()
    {
        if (!string.IsNullOrWhiteSpace(Title))
            await Clipboard.Default.SetTextAsync(Title);
    }

    [RelayCommand]
    private void ToggleSelectAll()
    {
        var isSelected = !AreAllVideosSelected;
        foreach (var item in Items)
            item.IsSelected = isSelected;
    }

    private bool CanConfirm() => SelectedVideos.Any();

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task ConfirmAsync()
    {
        if (!await downloadStorageService.EnsureStoragePermissionAsync())
        {
            snackbarManager.Notify(LocalizationManager.StoragePermissionDeniedMessage);
            return;
        }

        var existingFileNames = settingsService.ShouldSkipExistingFiles
            ? await Task.Run(downloadStorageService.GetExistingFileNames)
            : (IReadOnlySet<string>)new HashSet<string>();

        var downloads = new List<DownloadViewModel>();
        foreach (var (i, video) in SelectedVideos.Index())
        {
            var fileName = FileNameTemplate.Apply(
                settingsService.FileNameTemplate,
                video,
                SelectedContainer,
                (i + 1).ToString().PadLeft(SelectedVideos.Count.ToString().Length, '0')
            );

            if (
                settingsService.ShouldSkipExistingFiles
                && existingFileNames.Contains(DownloadStorageService.TruncateFileName(fileName))
            )
                continue;

            downloads.Add(
                viewModelManager.GetDownloadViewModel(
                    video,
                    new VideoDownloadPreference(SelectedContainer, SelectedVideoQualityPreference),
                    fileName
                )
            );
        }

        settingsService.LastContainer = SelectedContainer;
        settingsService.LastVideoQualityPreference = SelectedVideoQualityPreference;

        Close(downloads);
    }
}
