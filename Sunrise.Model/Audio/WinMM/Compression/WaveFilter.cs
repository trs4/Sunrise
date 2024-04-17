using System.Runtime.InteropServices;

namespace Sunrise.Model.Audio.Compression;

/// <summary>Summary description for WaveFilter</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public class WaveFilter
{
    /// <summary>cbStruct</summary>
    public int StructureSize = Marshal.SizeOf(typeof(WaveFilter));

    /// <summary>dwFilterTag</summary>
    public int FilterTag;

    /// <summary>fdwFilter</summary>
    public int Filter;

    /// <summary>reserved</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public int[]? Reserved;
}
