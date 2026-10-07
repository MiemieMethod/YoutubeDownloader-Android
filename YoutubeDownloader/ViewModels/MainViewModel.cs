using YoutubeDownloader.Framework;
using YoutubeDownloader.Localization;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels.Components;

namespace YoutubeDownloader.ViewModels;

public partial class MainViewModel(
    ViewModelManager viewModelManager,
    DialogManager dialogManager,
    SnackbarManager snackbarManager,
    LocalizationManager localizationManager,
    SettingsService settingsService,
    UpdateService updateService
) : ViewModelBase
{
    private bool _isInitialized;

    public string Title { get; } = $"{Program.Name} v{Program.VersionString}";

    public DashboardViewModel Dashboard { get; } = viewModelManager.GetDashboardViewModel();

    private async Task ShowUkraineSupportMessageAsync()
    {
        if (!settingsService.IsUkraineSupportMessageEnabled)
            return;

        var dialog = viewModelManager.GetMessageBoxViewModel(
            localizationManager.UkraineSupportTitle,
            localizationManager.UkraineSupportMessage,
            localizationManager.LearnMoreButton,
            localizationManager.CloseButton
        );

        // Disable this message in the future
        settingsService.IsUkraineSupportMessageEnabled = false;
        settingsService.Save();

        if (await dialogManager.ShowDialogAsync(dialog) == true)
            await Launcher.Default.OpenAsync("https://tyrrrz.me/ukraine?source=youtubedownloader");
    }

    // Notifications are used to display the progress of active downloads
    private static async Task RequestNotificationPermissionAsync()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            return;

        try
        {
            if (
                await Permissions.CheckStatusAsync<Permissions.PostNotifications>()
                != PermissionStatus.Granted
            )
            {
                await Permissions.RequestAsync<Permissions.PostNotifications>();
            }
        }
        catch
        {
            // Notifications are not critical
        }
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            var updateVersion = await updateService.CheckForUpdatesAsync();
            if (updateVersion is null)
                return;

            snackbarManager.Notify(
                string.Format(
                    localizationManager.UpdateAvailableMessage,
                    Program.Name,
                    updateVersion.ToString(3)
                ),
                localizationManager.DownloadButton,
                () => _ = Launcher.Default.OpenAsync(Program.ProjectReleasesUrl),
                TimeSpan.FromSeconds(15)
            );
        }
        catch
        {
            // Failure to check for updates shouldn't crash the application
            snackbarManager.Notify(localizationManager.UpdateCheckFailedMessage);
        }
    }

    public override async Task InitializeAsync()
    {
        // The view may be recreated by the system (e.g. when the activity is restarted),
        // but the initialization should only happen once.
        if (_isInitialized)
            return;

        _isInitialized = true;

        await ShowUkraineSupportMessageAsync();
        await Dashboard.InitializeAsync();
        await RequestNotificationPermissionAsync();
        await CheckForUpdatesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Save settings
            settingsService.Save();
        }

        base.Dispose(disposing);
    }
}
