using System.Net;
using System.Net.Http.Headers;

namespace YoutubeDownloader.Services;

// Upstream uses Onova to install updates automatically, which is not possible on Android.
// Instead, the latest release is looked up on GitHub and the user is offered to download it.
public class UpdateService(SettingsService settingsService)
{
    // The latest release is resolved from the redirect of the "releases/latest" page instead of
    // GitHub's REST API, which only allows 60 unauthenticated requests per hour per IP address.
    // That limit is easily exhausted on shared networks (VPNs, proxies, mobile carriers).
    private static readonly HttpClient Client = new(
        new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }
    )
    {
        Timeout = TimeSpan.FromSeconds(30),
        DefaultRequestHeaders =
        {
            UserAgent = { new ProductInfoHeaderValue(Program.Name, Program.VersionString) },
        },
    };

    private static async Task<Version?> TryGetLatestVersionAsync(
        CancellationToken cancellationToken = default
    )
    {
        using var response = await Client.GetAsync(
            Program.ProjectLatestReleaseUrl,
            cancellationToken
        );

        // The repository is not public, so its releases can't be accessed
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        var location = response.Headers.Location;
        if (location is null)
        {
            response.EnsureSuccessStatusCode();

            throw new HttpRequestException(
                $"Unexpected response from GitHub: {(int)response.StatusCode} {response.ReasonPhrase}."
            );
        }

        if (!location.IsAbsoluteUri)
            location = new Uri(new Uri(Program.ProjectLatestReleaseUrl), location);

        // The page redirects to "releases/tag/<tag>" if there is a release,
        // or to the list of releases if nothing has been published yet.
        var segments = location.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (
            segments.Length < 2
            || !string.Equals(segments[^2], "tag", StringComparison.OrdinalIgnoreCase)
        )
        {
            return null;
        }

        var tagName = Uri.UnescapeDataString(segments[^1]);
        return Version.TryParse(tagName.TrimStart('v', 'V'), out var version) ? version : null;
    }

    public async Task<Version?> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        if (!settingsService.IsAutoUpdateEnabled)
            return null;

        // Network requests are not allowed on the main thread
        var version = await Task.Run(
            () => TryGetLatestVersionAsync(cancellationToken),
            cancellationToken
        );

        if (version is null)
            return null;

        static Version Normalize(Version v) => new(v.Major, v.Minor, Math.Max(v.Build, 0));

        var latestVersion = Normalize(version);
        return latestVersion > Normalize(Program.Version) ? latestVersion : null;
    }
}
