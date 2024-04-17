using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Sunrise.Services;

public static class AppServices
{
    public static void Configure(Action<ServiceCollection> register)
    {
        var services = new ServiceCollection();
        register(services);
        var serviceProvider = services.BuildServiceProvider();
        Ioc.Default.ConfigureServices(serviceProvider);
    }

    public static T Get<T>() where T : class => Ioc.Default.GetRequiredService<T>();
}
