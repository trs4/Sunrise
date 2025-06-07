using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Sunrise.Converters;

public sealed class DurationConverter : IValueConverter
{
    public static readonly DurationConverter Instance = new();

    public static string Convert(TimeSpan value)
        => $"{(int)value.TotalMinutes}:{value.Seconds:00}";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is TimeSpan timeSpan ? Convert(timeSpan) : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
