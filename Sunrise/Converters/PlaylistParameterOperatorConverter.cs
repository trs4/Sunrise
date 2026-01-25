using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Sunrise.Model;

namespace Sunrise.Converters;

public sealed class PlaylistParameterOperatorConverter : IValueConverter
{
    public static readonly PlaylistParameterOperatorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is PlaylistParameterOperator @operator ? @operator.GetName() : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
