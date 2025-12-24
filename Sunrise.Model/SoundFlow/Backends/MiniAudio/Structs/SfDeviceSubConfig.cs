using System.Runtime.InteropServices;
using Sunrise.Model.SoundFlow.Backends.MiniAudio.Enums;
using Sunrise.Model.SoundFlow.Enums;

namespace Sunrise.Model.SoundFlow.Backends.MiniAudio.Structs;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct SfDeviceSubConfig
{
    public SampleFormat Format;
    public uint Channels;
    public nint pDeviceID;
    public ShareMode ShareMode;
}