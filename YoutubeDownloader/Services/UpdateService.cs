using System.Net;
using System.Text.Json;
using YoutubeDownloader.Core.Utils;

namespace YoutubeDownloader.Services;

// Upstream uses Onova to install updates automatically, which is not possible on Android.
// Instead, the latest release is looked up on GitHub and the user is offered to download it.
public class UpdateService(SettingsService settingsService)
{
    private static readonly Uri LatestReleaseApiUri = new(
        "https://api.github.com/repos/MiemieMethod/YoutubeDownloader-Android/releases/latest"
    );

    public async Task<Version?> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        if (!settingsService.IsAutoUpdateEnabled)
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Get, LatestReleaseApiUri);
        request.Headers.Accept.ParseAdd("application/vnd.github+json");

        using var response = await Http.Client.SendAsync(request, cancellationToken);

        // No releases have been published yet
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken
        );

        var tagName = document.RootElement.TryGetProperty("tag_name", out var tagNameElement)
            ? tagNameElement.GetString()
            : null;

        if (!Version.TryParse(tagName?.TrimStart('v', 'V'), out var version))
            return null;

        static Version Normalize(Version v) => new(v.Major, v.Minor, Math.Max(v.Build, 0));

        var latestVersion = Normalize(version);
        return latestVersion > Normalize(Program.Version) ? latestVersion : null;
    }
}
