using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Sunrise.Converters;

public class IsSmallerOrEqualConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        => values[0] is int firstNumber && values[1] is int secondNumber && firstNumber <= secondNumber;
}
