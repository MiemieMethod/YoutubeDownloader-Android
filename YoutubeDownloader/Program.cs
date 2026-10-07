using System;
using System.Reflection;

namespace YoutubeDownloader;

public static class Program
{
    private static Assembly Assembly { get; } = typeof(Program).Assembly;

    public static string Name { get; } = "YoutubeDownloader";

    public static Version Version { get; } = Assembly.GetName().Version ?? new Version(0, 0, 0);

    public static string VersionString { get; } = Version.ToString(3);

    public static string ProjectUrl { get; } =
        "https://github.com/MiemieMethod/YoutubeDownloader-Android";

    public static string ProjectReleasesUrl { get; } = $"{ProjectUrl}/releases";

    public static string ProjectLatestReleaseUrl { get; } = $"{ProjectReleasesUrl}/latest";

    public static string UpstreamProjectUrl { get; } = "https://github.com/Tyrrrz/YoutubeDownloader";
}
