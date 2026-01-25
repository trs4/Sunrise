using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Sunrise.Model;

namespace Sunrise.Converters;

public sealed class PlaylistParameterConverter : IValueConverter
{
    public static readonly PlaylistParameterConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is PlaylistParameter playlistParameter ? playlistParameter.GetName() : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
