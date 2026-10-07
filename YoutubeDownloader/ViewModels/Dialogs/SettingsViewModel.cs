using CommunityToolkit.Mvvm.Input;
using PowerKit.Extensions;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Localization;
using YoutubeDownloader.Services;

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class SettingsViewModel : DialogViewModelBase
{
    private readonly DialogManager _dialogManager;
    private readonly ViewModelManager _viewModelManager;
    private readonly SettingsService _settingsService;
    private readonly DownloadStorageService _downloadStorageService;

    private readonly IDisposable _eventSubscription;

    public SettingsViewModel(
        DialogManager dialogManager,
        ViewModelManager viewModelManager,
        LocalizationManager localizationManager,
        SettingsService settingsService,
        DownloadStorageService downloadStorageService
    )
    {
        _dialogManager = dialogManager;
        _viewModelManager = viewModelManager;
        LocalizationManager = localizationManager;
        _settingsService = settingsService;
        _downloadStorageService = downloadStorageService;

        _eventSubscription = _settingsService.WatchAllProperties(OnAllPropertiesChanged);
    }

    public LocalizationManager LocalizationManager { get; }

    public IReadOnlyList<ThemeVariant> AvailableThemes { get; } = Enum.GetValues<ThemeVariant>();

    public IReadOnlyList<string> AvailableThemeNames =>
        AvailableThemes.Select(t => t.ToString()).ToArray();

    public int ThemeIndex
    {
        get => AvailableThemes.ToList().IndexOf(_settingsService.Theme);
        set
        {
            if (value >= 0 && value < AvailableThemes.Count)
                _settingsService.Theme = AvailableThemes[value];
        }
    }

    public IReadOnlyList<Language> AvailableLanguages { get; } = Enum.GetValues<Language>();

    public IReadOnlyList<string> AvailableLanguageNames =>
        AvailableLanguages
            .Select(l =>
                l switch
                {
                    Language.ChineseSimplified => "Simplified Chinese",
                    _ => l.ToString(),
                }
            )
            .ToArray();

    public int LanguageIndex
    {
        get => AvailableLanguages.ToList().IndexOf(_settingsService.Language);
        set
        {
            if (value >= 0 && value < AvailableLanguages.Count)
                _settingsService.Language = AvailableLanguages[value];
        }
    }

    public bool IsAutoUpdateEnabled
    {
        get => _settingsService.IsAutoUpdateEnabled;
        set => _settingsService.IsAutoUpdateEnabled = value;
    }

    public bool IsAuthPersisted
    {
        get => _settingsService.IsAuthPersisted;
        set => _settingsService.IsAuthPersisted = value;
    }

    public bool ShouldInjectLanguageSpecificAudioStreams
    {
        get => _settingsService.ShouldInjectLanguageSpecificAudioStreams;
        set => _settingsService.ShouldInjectLanguageSpecificAudioStreams = value;
    }

    public bool ShouldInjectSubtitles
    {
        get => _settingsService.ShouldInjectSubtitles;
        set => _settingsService.ShouldInjectSubtitles = value;
    }

    public bool ShouldInjectTags
    {
        get => _settingsService.ShouldInjectTags;
        set => _settingsService.ShouldInjectTags = value;
    }

    public bool ShouldSkipExistingFiles
    {
        get => _settingsService.ShouldSkipExistingFiles;
        set => _settingsService.ShouldSkipExistingFiles = value;
    }

    public string FileNameTemplate
    {
        get => _settingsService.FileNameTemplate;
        set => _settingsService.FileNameTemplate = value;
    }

    public int ParallelLimit
    {
        get => _settingsService.ParallelLimit;
        set => _settingsService.ParallelLimit = Math.Clamp(value, 1, 10);
    }

    // Stepper operates on doubles
    public double ParallelLimitValue
    {
        get => ParallelLimit;
        set => ParallelLimit = (int)Math.Round(value);
    }

    public string DownloadDirectoryName => _downloadStorageService.DirectoryDisplayName;

    public bool IsDownloadDirectoryCustom => _settingsService.DownloadDirectoryUri is not null;

    [RelayCommand]
    private async Task BrowseDownloadDirectoryAsync()
    {
        try
        {
            var treeUri = await DownloadStorageService.PickDirectoryAsync();
            if (treeUri is null)
                return;

            _downloadStorageService.SetDirectory(treeUri);
        }
        catch (Exception ex)
        {
            await _dialogManager.ShowDialogAsync(
                _viewModelManager.GetMessageBoxViewModel(LocalizationManager.ErrorTitle, ex.Message)
            );
        }
    }

    [RelayCommand]
    private void ResetDownloadDirectory() => _downloadStorageService.SetDirectory(null);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _eventSubscription.Dispose();
        }

        base.Dispose(disposing);
    }
}
