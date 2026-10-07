using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using YoutubeDownloader.Services;

namespace YoutubeDownloader;

[Service(Exported = false, ForegroundServiceType = ForegroundService.TypeDataSync)]
public class DownloadService : Service
{
    private const int NotificationId = 1;
    private const string NotificationChannelId = "downloads";

    private PowerManager.WakeLock? _wakeLock;

    private static DownloadServiceManager? Manager =>
        IPlatformApplication.Current?.Services.GetService<DownloadServiceManager>();

    private static void EnsureNotificationChannel(Context context, DownloadServiceManager? manager)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
            return;

        if (context.GetSystemService(NotificationService) is not NotificationManager notificationManager)
            return;

        var channel = new NotificationChannel(
            NotificationChannelId,
            manager?.NotificationChannelName ?? "Downloads",
            NotificationImportance.Low
        );

        notificationManager.CreateNotificationChannel(channel);
    }

    private static Notification BuildNotification(Context context, DownloadServiceManager? manager)
    {
        EnsureNotificationChannel(context, manager);

        var launchIntent = new Intent(context, typeof(MainActivity));
        launchIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ReorderToFront);

        var pendingIntent = PendingIntent.GetActivity(
            context,
            0,
            launchIntent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent
        );

        var progress = (int)Math.Round(Math.Clamp(manager?.Progress ?? 0, 0, 1) * 100);

        var builder = new NotificationCompat.Builder(context, NotificationChannelId);
        builder.SetSmallIcon(global::Android.Resource.Drawable.StatSysDownload);
        builder.SetContentTitle(manager?.NotificationTitle ?? Program.Name);
        builder.SetContentText(manager?.NotificationText);
        builder.SetContentIntent(pendingIntent);
        builder.SetOngoing(true);
        builder.SetOnlyAlertOnce(true);
        builder.SetSilent(true);
        builder.SetProgress(100, progress, progress <= 0);
        builder.SetForegroundServiceBehavior(NotificationCompat.ForegroundServiceImmediate);

        return builder.Build()!;
    }

    internal static void UpdateNotification(DownloadServiceManager manager)
    {
        var context = Platform.AppContext;
        var notificationManager = NotificationManagerCompat.From(context)!;

        // Posting notifications requires a runtime permission on Android 13+
        if (!notificationManager.AreNotificationsEnabled())
            return;

        try
        {
            notificationManager.Notify(NotificationId, BuildNotification(context, manager));
        }
        catch (Java.Lang.SecurityException)
        {
            // Notification permission has been revoked
        }
    }

    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(
        Intent? intent,
        StartCommandFlags flags,
        int startId
    )
    {
        var notification = BuildNotification(this, Manager);

        if (OperatingSystem.IsAndroidVersionAtLeast(29))
            StartForeground(NotificationId, notification, ForegroundService.TypeDataSync);
        else
            StartForeground(NotificationId, notification);

        // Keep the CPU running while downloading, even if the screen is turned off
        if (_wakeLock is null && GetSystemService(PowerService) is PowerManager powerManager)
        {
            _wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "YoutubeDownloader:Download");
            _wakeLock?.SetReferenceCounted(false);
            _wakeLock?.Acquire();
        }

        return StartCommandResult.NotSticky;
    }

    public override void OnTimeout(int startId, ForegroundService fgsType)
    {
        // The system limits how long data sync foreground services can run (Android 15+)
        Manager?.OnServiceStopped();
        StopSelf();
    }

    public override void OnDestroy()
    {
        if (_wakeLock?.IsHeld == true)
            _wakeLock.Release();

        _wakeLock = null;

        base.OnDestroy();
    }
}
