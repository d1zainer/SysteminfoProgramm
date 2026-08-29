using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using SystemProgramm.Models;

namespace SystemProgramm.Views.Converters;

public sealed class PageKindToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not PageKind kind || Application.Current is not { } app)
            return null;

        return app.Resources.TryGetResource($"Icon.{kind}", app.ActualThemeVariant, out var icon)
            ? icon
            : null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}