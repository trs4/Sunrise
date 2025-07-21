using System;
using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Sunrise.Converters;

public sealed class AnyConverter : IValueConverter
{
    public static readonly AnyConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ICollection collection)
            return collection.Count > 0;
        else if (value is IEnumerable enumerable)
            return enumerable.GetEnumerator().MoveNext();

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
