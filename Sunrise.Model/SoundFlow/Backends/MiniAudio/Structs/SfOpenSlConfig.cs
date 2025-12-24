using System.Runtime.InteropServices;
using Sunrise.Model.SoundFlow.Backends.MiniAudio.Enums;

namespace Sunrise.Model.SoundFlow.Backends.MiniAudio.Structs;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct SfOpenSlConfig
{
    public OpenSlStreamType StreamType;
    public OpenSlRecordingPreset RecordingPreset;
}