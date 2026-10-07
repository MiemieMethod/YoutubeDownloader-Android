using System.Globalization;

namespace YoutubeDownloader.Converters;

public class IsZeroConverter(bool isInverted) : IValueConverter
{
    public static IsZeroConverter IsZero { get; } = new(false);

    public static IsZeroConverter IsNotZero { get; } = new(true);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (value is 0) != isInverted;

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
