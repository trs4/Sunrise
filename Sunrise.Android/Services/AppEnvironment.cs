using Sunrise.Services;
using Build = Android.OS.Build;

namespace Sunrise.Android.Services;

internal sealed class AppEnvironment : IAppEnvironment
{
    public string MachineName => Build.Model ?? "Android";
}
