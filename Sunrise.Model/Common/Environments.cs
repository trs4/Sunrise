using System.Runtime.InteropServices;

namespace Sunrise.Model.Common;

public static class Environments
{
    public static string GetPlatformName()
    {
        if (OperatingSystem.IsWindows())
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X86 => "windows-x86",
                Architecture.X64 => "windows-x64",
                Architecture.Arm64 => "windows-arm64",
                _ => "windows",
            };
        }
        else if (OperatingSystem.IsAndroid())
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "android-x64",
                Architecture.Arm => "android-arm",
                Architecture.Arm64 => "android-arm64",
                _ => "android",
            };
        }

        return "unknown";
    }

}
