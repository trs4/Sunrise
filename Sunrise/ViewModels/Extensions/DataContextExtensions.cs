using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Sunrise.ViewModels;

public static class DataContextExtensions
{
    private static int _prevPointerId;
    private static readonly object _canClickSync = new();

    public static T? FindDataContext<T>(this RoutedEventArgs e)
        where T : class
    {
        if (e.Source is not StyledElement element)
            return null;

        do
        {
            if (element.DataContext is T dataContext)
                return dataContext;

            element = element.Parent;
        }
        while (element is not null);

        return null;
    }

    public static bool CanClick(this TappedEventArgs e)
    {
        lock (_canClickSync)
        {
            int pointerId = (e.Pointer as Pointer)?.Id ?? 0;
            bool result = _prevPointerId != pointerId;
            _prevPointerId = pointerId;
            return result;
        }
    }

    public static T? FindDataContextWithCheck<T>(this TappedEventArgs e)
        where T : class
        => CanClick(e) ? e.FindDataContext<T>() : null;

    public static T? GetDataContext<T>(this RoutedEventArgs e)
        where T : class
        => (e.Source as StyledElement)?.DataContext as T ?? (e.Source as ContentPresenter)?.Content as T;

    public static T? GetDataContextWithCheck<T>(this TappedEventArgs e)
        where T : class
        => CanClick(e) ? e.GetDataContext<T>() : null;

    public static T? GetSelectedItem<T>(this SelectionChangedEventArgs e)
        where T : class
        => e.AddedItems?.Count > 0 ? e.AddedItems[0] as T : null;
}
