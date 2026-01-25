using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Sunrise.Model;

namespace Sunrise.Converters;

public sealed class PlaylistParameterSortingConverter : IValueConverter
{
    public static readonly PlaylistParameterSortingConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is PlaylistParameterSorting sorting ? sorting.GetName() : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
