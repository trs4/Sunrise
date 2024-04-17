using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Sunrise.Converters;

public sealed class ProgressConverter : IMultiValueConverter
{
    public static readonly ProgressConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2 || values[0] is not TimeSpan position || values[1] is not TimeSpan duration)
            return null;

        double progress = 100d * position.Ticks / duration.Ticks;

        if (progress < 0d)
            return 0d;
        else if (progress > 100d)
            return 100d;

        return progress;
    }

}
