using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SystemProgramm.Views.Converters;

public sealed class ListHalfConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable items)
        {
            return Array.Empty<object>();
        }

        var all = items.Cast<object>().ToArray();
        var half = (all.Length + 1) / 2;

        return parameter as string == "second" ? all[half..] : all[..half];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}