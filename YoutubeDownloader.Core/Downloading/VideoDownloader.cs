using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Gress;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeDownloader.Core.Downloading;

public class VideoDownloader : IDisposable
{
    private readonly HttpClient _http;
    private readonly YoutubeClient _youtube;

    public VideoDownloader(IReadOnlyList<Cookie>? initialCookies = null)
    {
        _http = Http.CreateYoutubeClient(initialCookies);
        _youtube = new YoutubeClient(_http, initialCookies ?? []);
    }

    public async Task<IReadOnlyList<VideoDownloadOption>> GetDownloadOptionsAsync(
        VideoId videoId,
        bool includeLanguageSpecificAudioStreams = true,
        CancellationToken cancellationToken = default
    )
    {
        var manifest = await _youtube.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
        return VideoDownloadOption.ResolveAll(manifest, includeLanguageSpecificAudioStreams);
    }

    public async Task<VideoDownloadOption> GetBestDownloadOptionAsync(
        VideoId videoId,
        VideoDownloadPreference preference,
        bool includeLanguageSpecificAudioStreams = true,
        CancellationToken cancellationToken = default
    )
    {
        var options = await GetDownloadOptionsAsync(
            videoId,
            includeLanguageSpecificAudioStreams,
            cancellationToken
        );

        return preference.TryGetBestOption(options)
            ?? throw new InvalidOperationException("No suitable download option found.");
    }

    // Subtitles are not essential, so failing to get them shouldn't fail the download
    private async Task<IReadOnlyList<ClosedCaptionTrackInfo>> TryGetClosedCaptionTracksAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default
    )
    {
        var trackInfos = new List<ClosedCaptionTrackInfo>();

        try
        {
            var manifest = await _youtube.Videos.ClosedCaptions.GetManifestAsync(
                videoId,
                cancellationToken
            );

            foreach (var trackInfo in manifest.Tracks)
            {
                try
                {
                    // Make sure the track can actually be retrieved
                    await _youtube.Videos.ClosedCaptions.GetAsync(trackInfo, cancellationToken);
                    trackInfos.Add(trackInfo);
                }
                catch (Exception) when (!cancellationToken.IsCancellationRequested) { }
            }
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested) { }

        return trackInfos;
    }

    public async Task DownloadVideoAsync(
        string filePath,
        IVideo video,
        VideoDownloadOption downloadOption,
        bool includeSubtitles = true,
        string? ffmpegPath = null,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        // Include subtitles in the output container
        var trackInfos = new List<ClosedCaptionTrackInfo>();
        if (includeSubtitles && !downloadOption.Container.IsAudioOnly)
            trackInfos.AddRange(await TryGetClosedCaptionTracksAsync(video.Id, cancellationToken));

        var dirPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(dirPath))
            Directory.CreateDirectory(dirPath);

        await _youtube.Videos.DownloadAsync(
            downloadOption.StreamInfos,
            trackInfos,
            new ConversionRequestBuilder(filePath)
                // On Android, FFmpeg is bundled with the app as a native executable,
                // so the caller is expected to always provide its path
                .SetFFmpegPath(ffmpegPath ?? "ffmpeg")
                .SetContainer(downloadOption.Container)
                .SetPreset(ConversionPreset.Medium)
                .Build(),
            progress?.ToDoubleBased(),
            cancellationToken
        );
    }

    public void Dispose()
    {
        _youtube.Dispose();
        _http.Dispose();
    }
}
