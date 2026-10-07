using Google.Android.Material.Snackbar;

namespace YoutubeDownloader.Framework;

public class SnackbarManager
{
    private readonly TimeSpan _defaultDuration = TimeSpan.FromSeconds(5);

    public void Notify(string message, TimeSpan? duration = null) =>
        Notify(message, null, null, duration);

    public void Notify(
        string message,
        string? actionText,
        Action? actionHandler,
        TimeSpan? duration = null
    )
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var view = Platform.CurrentActivity?.FindViewById(global::Android.Resource.Id.Content);
            if (view is null)
                return;

            var snackbar = Snackbar.Make(
                view,
                message,
                (int)(duration ?? _defaultDuration).TotalMilliseconds
            );

            if (!string.IsNullOrWhiteSpace(actionText) && actionHandler is not null)
            {
                snackbar.SetAction(actionText, _ => actionHandler());
                snackbar.SetActionTextColor(global::Android.Graphics.Color.ParseColor("#F9A825"));
            }

            // Allow longer messages (e.g. errors) to be displayed in full
            if (
                snackbar.View.FindViewById<global::Android.Widget.TextView>(
                    Resource.Id.snackbar_text
                )
                is { } textView
            )
            {
                textView.SetMaxLines(6);
            }

            snackbar.Show();
        });
    }
}
