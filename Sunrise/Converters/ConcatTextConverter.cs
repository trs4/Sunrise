using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Sunrise.Converters;

public sealed class ConcatTextConverter : IMultiValueConverter
{
    public static readonly ConcatTextConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2 || values[0] is not string str1 || values[1] is not string str2)
            return null;

        if (string.IsNullOrWhiteSpace(str1))
            str1 = null;

        if (string.IsNullOrWhiteSpace(str2))
            str2 = null;

        return str1 is null ? str2 : (str2 is null ? str1 : $"{str1} - {str2}");
    }

}
