using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class CategoriesTabView : UserControl
{
    public CategoriesTabView()
        => InitializeComponent();

    private async void Category_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Tab)
            await OnCategoryNameChanged(e);
    }

    private async void Category_LostFocus(object? sender, RoutedEventArgs e)
        => await OnCategoryNameChanged(e);

    private Task OnCategoryNameChanged(RoutedEventArgs e)
    {
        if (DataContext is not MainDeviceViewModel mainViewModel)
            return Task.CompletedTask;

        return mainViewModel.OnApplyCategoryAsync();
    }

}