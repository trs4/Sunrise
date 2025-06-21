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
        MainViewModel viewModel = null;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) // Windows
        {
            //desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            //viewModel = new MainDesktopViewModel(player);
            //desktop.MainWindow = ((MainDesktopViewModel)viewModel).Owner = new MainWindow { DataContext = viewModel };

            viewModel = new MainDeviceViewModel(player);
            desktop.MainWindow = new MainDeviceWindow { DataContext = viewModel };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) // Android
        {
            viewModel = new MainDeviceViewModel(player);
            singleViewPlatform.MainView = new MainDeviceView { DataContext = viewModel };
        }

        if (viewModel is not null)
            await viewModel.ReloadTracksAsync();

        base.OnFrameworkInitializationCompleted();
    }

}
