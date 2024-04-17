using Microsoft.Extensions.DependencyInjection;
using Sunrise.Services;

namespace Sunrise.Desktop.Services;

internal static class DesktopServices
{
    public static void Configure() => AppServices.Configure(Register);
    
    private static void Register(ServiceCollection services)
        => services.AddSingleton<ISystemDialogsService, SystemDialogsService>();
}
