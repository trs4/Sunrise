using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Sunrise.Converters;

public sealed class RandomPlayOpacityConverter : IValueConverter
{
    public static readonly RandomPlayOpacityConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool boolValue && boolValue ? 1.0 : 0.4;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
