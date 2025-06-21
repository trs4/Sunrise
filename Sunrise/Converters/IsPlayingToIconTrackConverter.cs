using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Sunrise.Model.Resources;
using Sunrise.Utils;

namespace Sunrise.Converters;

public sealed class IsPlayingToIconTrackConverter : IValueConverter
{
    private static readonly object _play = IconSource.From(nameof(Icons.Play));
    private static readonly object _pause = IconSource.From(nameof(Icons.Pause));

    public static readonly IsPlayingToIconTrackConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return boolValue ? _pause : _play;

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
