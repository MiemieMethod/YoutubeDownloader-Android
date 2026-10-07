using System.Globalization;
using System.Text;

namespace YoutubeDownloader.Converters;

// Converts the subset of Markdown used in localization strings (**bold** and *italic*)
// into formatted text
public class MarkdownToFormattedStringConverter : IValueConverter
{
    public static MarkdownToFormattedStringConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var formattedString = new FormattedString();
        if (value is not string { Length: > 0 } text)
            return formattedString;

        var buffer = new StringBuilder();
        var isBold = false;
        var isItalic = false;

        void Flush()
        {
            if (buffer.Length == 0)
                return;

            formattedString.Spans.Add(
                new Span
                {
                    Text = buffer.ToString(),
                    FontAttributes =
                        (isBold ? FontAttributes.Bold : FontAttributes.None)
                        | (isItalic ? FontAttributes.Italic : FontAttributes.None),
                }
            );

            buffer.Clear();
        }

        var normalizedText = text.Replace("\r\n", "\n").Trim();
        for (var i = 0; i < normalizedText.Length; i++)
        {
            var c = normalizedText[i];

            if (c == '*' && i + 1 < normalizedText.Length && normalizedText[i + 1] == '*')
            {
                Flush();
                isBold = !isBold;
                i++;
            }
            else if (
                c == '*'
                && (isItalic || (i + 1 < normalizedText.Length && !char.IsWhiteSpace(normalizedText[i + 1])))
            )
            {
                Flush();
                isItalic = !isItalic;
            }
            else
            {
                buffer.Append(c);
            }
        }

        Flush();

        return formattedString;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) => throw new NotSupportedException();
}
