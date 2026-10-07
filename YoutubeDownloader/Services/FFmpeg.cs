namespace YoutubeDownloader.Services;

// FFmpeg is bundled with the app as a native "library" (see ffmpeg/build.sh),
// which Android extracts into the app's native library directory upon installation.
public static class FFmpeg
{
    public static string CliFilePath { get; } =
        Path.Combine(
            Platform.AppContext.ApplicationInfo?.NativeLibraryDir ?? AppContext.BaseDirectory,
            "libffmpeg.so"
        );

    public static bool IsAvailable() => File.Exists(CliFilePath);
}
