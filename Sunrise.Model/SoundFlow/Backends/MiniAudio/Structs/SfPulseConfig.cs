using System.Runtime.InteropServices;

namespace Sunrise.Model.SoundFlow.Backends.MiniAudio.Structs;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct SfPulseConfig
{
    public nint pStreamNamePlayback;
    public nint pStreamNameCapture;
}