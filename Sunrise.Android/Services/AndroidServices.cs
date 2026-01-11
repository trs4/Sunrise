using Avalonia.Android;
using Microsoft.Extensions.DependencyInjection;
using Sunrise.Services;

namespace Sunrise.Android.Services;

internal static class AndroidServices
{
    public static void Configure(AvaloniaMainActivity activity)
        => AppServices.Configure(services => Register(services, activity));

    private static void Register(ServiceCollection services, AvaloniaMainActivity activity) => services
        .AddSingleton<IAppEnvironment>(new AppEnvironment(activity));
}