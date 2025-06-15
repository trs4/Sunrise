using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;

namespace Sunrise.ViewModels;

public static class DataContextExtensions
{
    public static T? GetDataContext<T>(this RoutedEventArgs e)
        where T : class
        => (e.Source as StyledElement)?.DataContext as T ?? (e.Source as ContentPresenter)?.Content as T;

    public static T? GetSelectedItem<T>(this SelectionChangedEventArgs e)
        where T : class
        => e.AddedItems?.Count > 0 ? e.AddedItems[0] as T : null;
}
