using Android.Content;
using YoutubeDownloader.Localization;

namespace YoutubeDownloader.Services;

// Keeps the app alive while downloads are active by running a foreground service,
// which also shows the overall download progress in a notification.
public class DownloadServiceManager(LocalizationManager localizationManager)
{
    private readonly Lock _lock = new();

    private bool _isRunning;
    private DateTimeOffset _lastNotificationUpdate = DateTimeOffset.MinValue;

    public int ActiveDownloadCount { get; private set; }

    public double Progress { get; private set; }

    public string NotificationChannelName => localizationManager.DownloadsNotificationChannelName;

    public string NotificationTitle => Program.Name;

    public string NotificationText =>
        string.Format(localizationManager.DownloadsNotificationText, ActiveDownloadCount);

    private static Context Context => Platform.AppContext;

    public void Update(int activeDownloadCount)
    {
        lock (_lock)
        {
            ActiveDownloadCount = activeDownloadCount;

            if (activeDownloadCount > 0 && !_isRunning)
            {
                try
                {
                    var intent = new Intent(Context, typeof(DownloadService));

                    if (OperatingSystem.IsAndroidVersionAtLeast(26))
                        Context.StartForegroundService(intent);
                    else
                        Context.StartService(intent);

                    _isRunning = true;
                }
                catch (Exception ex) when (ex is not OutOfMemoryException)
                {
                    // Foreground services cannot be started while the app is in background.
                    // Downloads will still work, but may be interrupted by the system.
                }
            }
            else if (activeDownloadCount <= 0 && _isRunning)
            {
                Context.StopService(new Intent(Context, typeof(DownloadService)));
                _isRunning = false;
            }
            else if (_isRunning)
            {
                DownloadService.UpdateNotification(this);
            }
        }
    }

    public void ReportProgress(double progress)
    {
        lock (_lock)
        {
            Progress = progress;

            // Throttle notification updates, as the system limits their rate
            var now = DateTimeOffset.UtcNow;
            if (!_isRunning || now - _lastNotificationUpdate < TimeSpan.FromSeconds(1))
                return;

            _lastNotificationUpdate = now;
            DownloadService.UpdateNotification(this);
        }
    }

    // Called when the system stops the service (e.g. foreground service timeout on Android 15+)
    internal void OnServiceStopped()
    {
        lock (_lock)
            _isRunning = false;
    }
}
