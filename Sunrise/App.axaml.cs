using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Sunrise.Model;
using Sunrise.ViewModels;
using Sunrise.Views;

namespace Sunrise;

public partial class App : Application
{
    public override void Initialize()
        => AvaloniaXamlLoader.Load(this);

    public override async void OnFrameworkInitializationCompleted()
    {
        BindingPlugins.DataValidators.RemoveAt(0);
        var player = await Player.InitAsync();
        var viewModel = new MainViewModel(player);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) // Windows
        {
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            //desktop.MainWindow = viewModel.Owner = new MainWindow { DataContext = viewModel };
            desktop.MainWindow = new MainDeviceWindow { DataContext = viewModel };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) // Android
            singleViewPlatform.MainView = new MainDeviceView { DataContext = viewModel };

        await viewModel.ReloadTracksAsync();
        base.OnFrameworkInitializationCompleted();
    }

}
