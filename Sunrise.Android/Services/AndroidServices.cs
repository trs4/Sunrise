using Microsoft.Extensions.DependencyInjection;
using Sunrise.Services;

namespace Sunrise.Android.Services;

internal static class AndroidServices
{
    public static void Configure() => AppServices.Configure(Register);

    private static void Register(ServiceCollection services) => services
        .AddSingleton<IAppEnvironment, AppEnvironment>();
}