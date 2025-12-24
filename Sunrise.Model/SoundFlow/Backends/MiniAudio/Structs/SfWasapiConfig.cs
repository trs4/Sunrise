using System.Runtime.InteropServices;
using Sunrise.Model.SoundFlow.Backends.MiniAudio.Enums;

namespace Sunrise.Model.SoundFlow.Backends.MiniAudio.Structs;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct SfWasapiConfig
{
    public WasapiUsage Usage;
    [MarshalAs(UnmanagedType.U1)] public bool NoAutoConvertSRC;
    [MarshalAs(UnmanagedType.U1)] public bool NoDefaultQualitySRC;
    [MarshalAs(UnmanagedType.U1)] public bool NoAutoStreamRouting;
    [MarshalAs(UnmanagedType.U1)] public bool NoHardwareOffloading;
}