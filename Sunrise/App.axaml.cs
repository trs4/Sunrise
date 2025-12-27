using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Sunrise.Model;
using Sunrise.Services;
using Sunrise.ViewModels;
using Sunrise.Views;

namespace Sunrise;

public partial class App : Application
{
    public override void Initialize()
        => AvaloniaXamlLoader.Load(this);

    public override async void OnFrameworkInitializationCompleted()
    {
        SetCurrentCulture();
        var player = await Player.InitAsync();
        var viewModel = InitApplication(player);
        await viewModel.ReloadTracksAsync();
        base.OnFrameworkInitializationCompleted();
    }

    private MainViewModel InitApplication(Player player)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return InitWindows(player, desktop);
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            return InitAndroid(player, singleViewPlatform);

        throw new NotSupportedException(ApplicationLifetime?.GetType().FullName);
    }

    private static MainDesktopViewModel InitWindows(Player player, IClassicDesktopStyleApplicationLifetime application)
    {
        DisableAvaloniaDataAnnotationValidation();

        application.ShutdownMode = ShutdownMode.OnMainWindowClose;
        var viewModel = new MainDesktopViewModel(player);
        application.MainWindow = viewModel.Owner = new MainWindow { DataContext = viewModel };
        AppServices.Get<IAppSyncService>().Start(viewModel.Dispatcher);
        application.ShutdownRequested += Desktop_ShutdownRequested;
        return viewModel;
    }

    //private static MainDeviceViewModel InitWindows(Player player, IClassicDesktopStyleApplicationLifetime application)
    //{
    //    DisableAvaloniaDataAnnotationValidation();

    //    var viewModel = new MainDeviceViewModel(player);
    //    application.MainWindow = new MainDeviceWindow { DataContext = viewModel };
    //    return viewModel;
    //}

    private static MainDeviceViewModel InitAndroid(Player player, ISingleViewApplicationLifetime application)
    {
        var viewModel = new MainDeviceViewModel(player);
        application.MainView = new MainDeviceView { DataContext = viewModel };
        return viewModel;
    }

    private static async void Desktop_ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        => await AppServices.Get<IAppSyncService>().ShutdownAsync();

    private static void SetCurrentCulture()
    {
        //var cultureInfo = new CultureInfo("ru-RU");
        var cultureInfo = new CultureInfo("en-US"); // FIX BUG

        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;

        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
            BindingPlugins.DataValidators.Remove(plugin);
    }

}
