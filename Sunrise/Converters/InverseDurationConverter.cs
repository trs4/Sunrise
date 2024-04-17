using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Sunrise.Converters;

public sealed class InverseDurationConverter : IMultiValueConverter
{
    public static readonly InverseDurationConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2 || values[0] is not TimeSpan position || values[1] is not TimeSpan duration)
            return null;

        var timeSpan = duration.Subtract(position);
        return timeSpan.Ticks < 0 ? "0:00" : $"{(int)timeSpan.TotalMinutes}:{timeSpan.Seconds:00}";
    }

}
