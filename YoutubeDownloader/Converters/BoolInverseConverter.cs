using System.Globalization;

namespace YoutubeDownloader.Converters;

public class BoolInverseConverter : IValueConverter
{
    public static BoolInverseConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not true;

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => value is not true;
}
