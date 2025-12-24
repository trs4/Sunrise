using System.Runtime.InteropServices;
using Sunrise.Model.SoundFlow.Backends.MiniAudio.Enums;

namespace Sunrise.Model.SoundFlow.Backends.MiniAudio.Structs;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal struct SfAAudioConfig
{
    public AAudioUsage Usage;
    public AAudioContentType ContentType;
    public AAudioInputPreset InputPreset;
    public AAudioAllowedCapturePolicy AllowedCapturePolicy;
}