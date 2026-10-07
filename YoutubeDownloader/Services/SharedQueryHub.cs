using System.Text.RegularExpressions;

namespace YoutubeDownloader.Services;

// Receives text shared to the app from other apps (e.g. the "Share" button in the YouTube app)
public static partial class SharedQueryHub
{
    private static readonly Lock SyncRoot = new();
    private static string? _pendingQuery;

    public static event EventHandler? QueryReceived;

    [GeneratedRegex(@"https?://\S+", RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();

    // Shared text often contains more than just the URL (e.g. "Video title https://youtu.be/...")
    private static string ExtractQuery(string text)
    {
        var urls = UrlRegex().Matches(text).Select(m => m.Value).ToArray();
        return urls.Length > 0 ? string.Join('\n', urls) : text.Trim();
    }

    public static void Publish(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        lock (SyncRoot)
            _pendingQuery = ExtractQuery(text);

        QueryReceived?.Invoke(null, EventArgs.Empty);
    }

    public static string? TryConsume()
    {
        lock (SyncRoot)
        {
            var query = _pendingQuery;
            _pendingQuery = null;
            return query;
        }
    }
}
