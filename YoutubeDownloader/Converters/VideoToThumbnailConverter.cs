using System.Globalization;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Converters;

public class VideoToThumbnailConverter(bool isHighestQuality) : IValueConverter
{
    public static VideoToThumbnailConverter Highest { get; } = new(true);

    public static VideoToThumbnailConverter Lowest { get; } = new(false);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IVideo video)
            return null;

        var url = isHighestQuality
            ? video.Thumbnails.TryGetWithHighestResolution()?.Url
            : video.Thumbnails.MinBy(t => t.Resolution.Area)?.Url;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            ? new UriImageSource { Uri = uri, CachingEnabled = true }
            : null;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
