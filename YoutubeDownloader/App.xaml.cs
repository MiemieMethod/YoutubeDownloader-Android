using PowerKit.Extensions;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.ViewModels;
using YoutubeDownloader.Views;

namespace YoutubeDownloader;

public partial class App : Application
{
    private readonly IServiceProvider _services;
    private readonly SettingsService _settingsService;

    private readonly IDisposable _eventSubscription;

    public App(IServiceProvider services, SettingsService settingsService)
    {
        _services = services;
        _settingsService = settingsService;

        // Load settings
        _settingsService.Load();

        InitializeComponent();

        // Apply the theme and re-apply it when the user changes it
        _eventSubscription = _settingsService.WatchProperty(
            o => o.Theme,
            v =>
                UserAppTheme = v switch
                {
                    ThemeVariant.Light => AppTheme.Light,
                    ThemeVariant.Dark => AppTheme.Dark,
                    _ => AppTheme.Unspecified,
                },
            true
        );
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // The window may be recreated by the system (e.g. after the activity has been destroyed),
        // but the view model is a singleton, so the application state is preserved.
        var viewModelManager = _services.GetRequiredService<ViewModelManager>();
        var mainPage = new MainPage { BindingContext = viewModelManager.GetMainViewModel() };

        return new Window(mainPage) { Title = Program.Name };
    }

    protected override void OnSleep()
    {
        // The app may be killed by the system at any moment while in background
        _settingsService.Save();

        base.OnSleep();
    }

    protected override void CleanUp()
    {
        _eventSubscription.Dispose();
        base.CleanUp();
    }
}
