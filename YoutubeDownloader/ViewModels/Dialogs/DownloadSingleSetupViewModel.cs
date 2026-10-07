using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerKit.Extensions;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Localization;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Components;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class DownloadSingleSetupViewModel(
    ViewModelManager viewModelManager,
    SnackbarManager snackbarManager,
    LocalizationManager localizationManager,
    SettingsService settingsService,
    DownloadStorageService downloadStorageService
) : DialogViewModelBase<DownloadViewModel>
{
    public LocalizationManager LocalizationManager { get; } = localizationManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DurationText))]
    public partial IVideo? Video { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AvailableDownloadOptionNames))]
    public partial IReadOnlyList<VideoDownloadOption>? AvailableDownloadOptions { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedDownloadOptionIndex))]
    public partial VideoDownloadOption? SelectedDownloadOption { get; set; }

    // Name of the output file without the extension, which is determined by the selected format
    [ObservableProperty]
    public partial string? FileName { get; set; }

    public string DurationText =>
        Video?.Duration is { } duration
            ? duration.ToString(duration.TotalHours >= 1 ? @"h\:mm\:ss" : @"m\:ss")
            : LocalizationManager.LiveLabel;

    public IReadOnlyList<string> AvailableDownloadOptionNames =>
        AvailableDownloadOptions?.Select(GetDisplayName).ToArray() ?? [];

    public int SelectedDownloadOptionIndex
    {
        get =>
            AvailableDownloadOptions is not null && SelectedDownloadOption is not null
                ? AvailableDownloadOptions.ToList().IndexOf(SelectedDownloadOption)
                : -1;
        set =>
            SelectedDownloadOption =
                AvailableDownloadOptions is not null
                && value >= 0
                && value < AvailableDownloadOptions.Count
                    ? AvailableDownloadOptions[value]
                    : null;
    }

    private string GetDisplayName(VideoDownloadOption option)
    {
        var quality = option.IsAudioOnly
            ? LocalizationManager.AudioLabel
            : option.VideoQuality?.Label ?? string.Empty;

        var upscaled = option.IsVideoUpscaled ? $" ({LocalizationManager.UpscaledLabel})" : "";

        return $"{quality}{upscaled} · {option.Container.Name}";
    }

    public override Task InitializeAsync()
    {
        SelectedDownloadOption =
            AvailableDownloadOptions?.FirstOrDefault(o =>
                o.Container == settingsService.LastContainer
            ) ?? AvailableDownloadOptions?.FirstOrDefault();

        if (Video is not null && SelectedDownloadOption is not null)
        {
            FileName = Path.GetFileNameWithoutExtension(
                FileNameTemplate.Apply(
                    settingsService.FileNameTemplate,
                    Video,
                    SelectedDownloadOption.Container
                )
            );
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task CopyTitleAsync()
    {
        if (!string.IsNullOrWhiteSpace(Video?.Title))
            await Clipboard.Default.SetTextAsync(Video.Title);
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (Video is null || SelectedDownloadOption is null)
            return;

        if (!await downloadStorageService.EnsureStoragePermissionAsync())
        {
            snackbarManager.Notify(LocalizationManager.StoragePermissionDeniedMessage);
            return;
        }

        var container = SelectedDownloadOption.Container;

        // Use the name entered by the user, if it's valid, or fall back to the template
        var fileName = !string.IsNullOrWhiteSpace(FileName)
            ? Path.EscapeFileName(FileName.Trim() + '.' + container.Name, true)
            : FileNameTemplate.Apply(settingsService.FileNameTemplate, Video, container);

        settingsService.LastContainer = container;

        Close(viewModelManager.GetDownloadViewModel(Video, SelectedDownloadOption, fileName));
    }
}
