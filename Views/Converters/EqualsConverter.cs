using System.Globalization;
using Avalonia.Data.Converters;

namespace SystemProgramm.Views.Converters;

// Сравнение значения с параметром: даёт классы вроде .temperature,
// не заводя во вью-моделях булево поле на каждый случай.
public sealed class EqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Equals(value, parameter);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
