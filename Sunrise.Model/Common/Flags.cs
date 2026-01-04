using System.Runtime.CompilerServices;

namespace Sunrise.Model.Common;

internal static class Flags
{
    public const MethodImplOptions HotPath = MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization;
}
