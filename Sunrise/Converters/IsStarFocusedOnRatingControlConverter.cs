using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using Avalonia.Data.Converters;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Sunrise.Controls;

namespace Sunrise.Converters;

public class IsStarFocusedOnRatingControlConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values[0] is not int position || values[1] is not RatingControl control || values[2] is not bool isPointerOver || !isPointerOver)
            return false;

        var focusedPosition = (control.GetVisualChildren().OfType<ContentControl>().FirstOrDefault()?.Content as ItemsControl)
            ?.GetLogicalChildren().OfType<ContentPresenter>().Select(c => c.Child).OfType<Path>().FirstOrDefault(p => p.IsPointerOver)
            ?.DataContext as int? ?? -1;

        return position <= focusedPosition;
    }

}
