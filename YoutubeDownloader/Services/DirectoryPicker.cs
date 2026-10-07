using Android.App;
using Android.Content;
using AndroidUri = Android.Net.Uri;

namespace YoutubeDownloader.Services;

// Lets the user pick a folder via the Storage Access Framework
public static class DirectoryPicker
{
    private const int RequestCode = 0x5944;

    private static TaskCompletionSource<AndroidUri?>? _pendingTcs;

    public static Task<AndroidUri?> PickAsync()
    {
        var activity =
            Platform.CurrentActivity
            ?? throw new InvalidOperationException("No activity is available.");

        _pendingTcs?.TrySetResult(null);
        var tcs = _pendingTcs = new TaskCompletionSource<AndroidUri?>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        var intent = new Intent(Intent.ActionOpenDocumentTree);
        intent.AddFlags(
            ActivityFlags.GrantReadUriPermission
                | ActivityFlags.GrantWriteUriPermission
                | ActivityFlags.GrantPersistableUriPermission
                | ActivityFlags.GrantPrefixUriPermission
        );

        activity.StartActivityForResult(intent, RequestCode);

        return tcs.Task;
    }

    public static void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        if (requestCode != RequestCode)
            return;

        var tcs = _pendingTcs;
        _pendingTcs = null;

        tcs?.TrySetResult(resultCode == Result.Ok ? data?.Data : null);
    }
}
