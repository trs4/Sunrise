using Avalonia.Controls;
using Sunrise.ViewModels;

namespace Sunrise.Views;

public partial class CalculatedPlaylistWindow : Window, ICalculatedPlaylistView
{
    public CalculatedPlaylistWindow()
        => InitializeComponent();

    void ICalculatedPlaylistView.Close() => Close();
}
