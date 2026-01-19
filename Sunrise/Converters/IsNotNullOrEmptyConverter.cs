using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Sunrise.Converters;

public sealed class IsNotNullOrEmptyConverter : IValueConverter
{
    public static readonly IsNotNullOrEmptyConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string str && str.Length > 0;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
