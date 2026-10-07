using System.Globalization;
using YoutubeDownloader.ViewModels.Components;

namespace YoutubeDownloader.Converters;

public class DownloadStatusToColorConverter : IValueConverter
{
    public static DownloadStatusToColorConverter Instance { get; } = new();

    private static Color GetSecondaryTextColor() =>
        Application.Current?.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#B3FFFFFF")
            : Color.FromArgb("#8A000000");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            DownloadStatus.Completed => Color.FromArgb("#43A047"),
            DownloadStatus.Canceled => Color.FromArgb("#FB8C00"),
            DownloadStatus.Failed => Color.FromArgb("#E53935"),
            _ => GetSecondaryTextColor(),
        };

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
