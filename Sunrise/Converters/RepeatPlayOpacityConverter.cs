using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Sunrise.Converters;

public sealed class RepeatPlayOpacityConverter : IValueConverter
{
    public static readonly RepeatPlayOpacityConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool ? 1.0 : 0.4;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
