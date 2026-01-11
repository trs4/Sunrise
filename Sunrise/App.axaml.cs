using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Sunrise.Model;
using Sunrise.Model.Common;
using Sunrise.Model.SoundFlow.Components;
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
        static string getDeviceName() => AppServices.Get<IAppEnvironment>().MachineName;

        SetCurrentCulture();
        InitExceptionHandlers();
        var player = await Player.InitAsync(getDeviceName);
        var viewModel = InitApplication(player);
        await viewModel.ReloadTracksAsync();
        _ = Tasks.StartOnDefaultScheduler(WavePlayer.Prepare);
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

    private static void InitExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private static async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        string message = e.ExceptionObject is Exception exception ? ExceptionHandler.GetString(exception) : e.ExceptionObject?.ToString();
        await MessageBoxManager.GetMessageBoxStandard("UnhandledException", message, ButtonEnum.Ok).ShowAsync();
    }

    private static async void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        string message = ExceptionHandler.GetString(e.Exception);
        await MessageBoxManager.GetMessageBoxStandard("UnobservedTaskException", message, ButtonEnum.Ok).ShowAsync();
        e.SetObserved();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
            BindingPlugins.DataValidators.Remove(plugin);
    }

}
