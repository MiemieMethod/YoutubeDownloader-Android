using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using YoutubeDownloader.Services;

namespace YoutubeDownloader;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTask,
    ConfigurationChanges = ConfigChanges.ScreenSize
        | ConfigChanges.Orientation
        | ConfigChanges.UiMode
        | ConfigChanges.ScreenLayout
        | ConfigChanges.SmallestScreenSize
        | ConfigChanges.Density
        | ConfigChanges.KeyboardHidden
        | ConfigChanges.Keyboard
        | ConfigChanges.Navigation
)]
// Allows sharing links (e.g. from the YouTube app) to this app
[IntentFilter(
    [Intent.ActionSend],
    Categories = [Intent.CategoryDefault],
    DataMimeType = "text/plain"
)]
public class MainActivity : MauiAppCompatActivity
{
    private static void HandleIntent(Intent? intent)
    {
        if (intent?.Action != Intent.ActionSend)
            return;

        var text = intent.GetStringExtra(Intent.ExtraText);
        if (!string.IsNullOrWhiteSpace(text))
            SharedQueryHub.Publish(text);
    }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Don't handle the same intent again when the activity is recreated
        if (savedInstanceState is null)
            HandleIntent(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        HandleIntent(intent);
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        DirectoryPicker.OnActivityResult(requestCode, resultCode, data);
    }
}
