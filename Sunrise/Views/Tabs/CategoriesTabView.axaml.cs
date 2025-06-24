using Avalonia.Controls;
using Avalonia.Input;

namespace Sunrise.Views;

public partial class CategoriesTabView : UserControl
{
    public CategoriesTabView()
        => InitializeComponent();

    private void Category_Tapped(object? sender, TappedEventArgs e)
    {
        //if (DataContext is not MainViewModel mainViewModel)
        //    return;

        //mainViewModel.IsCategoriesVisible = !mainViewModel.IsCategoriesVisible;
    }

}