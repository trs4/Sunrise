using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Sunrise.Model.Resources;
using Sunrise.Utils;

namespace Sunrise.Converters;

public sealed class IsPlayingToIconConverter : IValueConverter
{
    private static readonly object _play = IconSource.From(nameof(Icons.MutePlay));
    private static readonly object _stop = IconSource.From(nameof(Icons.MuteStop));

    public static readonly IsPlayingToIconConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return boolValue ? _play : _stop;

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
