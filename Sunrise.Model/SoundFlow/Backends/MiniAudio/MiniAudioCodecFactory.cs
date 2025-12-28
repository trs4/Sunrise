using Sunrise.Model.SoundFlow.Interfaces;
using Sunrise.Model.SoundFlow.Structs;

namespace Sunrise.Model.SoundFlow.Backends.MiniAudio;

/// <summary>
/// Implements the <see cref="ICodecFactory"/> for the formats natively supported by MiniAudio.
/// This factory is registered by the <see cref="MiniAudioEngine"/> at a low priority to act as a default/fallback
/// </summary>
public sealed class MiniAudioCodecFactory : ICodecFactory
{
    public string FactoryId => "SoundFlow.MiniAudio.Default";

    public IReadOnlyCollection<string> SupportedFormatIds { get; } = ["wav", "mp3", "flac"];

    public int Priority => 0; // Low priority, intended as a fallback

    public ISoundDecoder? CreateDecoder(Stream stream, string formatId, AudioFormat format)
        => SupportedFormatIds.Contains(formatId) ? new MiniAudioDecoder(stream, format.Format, format.Channels, format.SampleRate) : null;

    public ISoundDecoder TryCreateDecoder(Stream stream, out AudioFormat detectedFormat, AudioFormat? hintFormat = null)
    {
        // MiniAudio does not support probing, so we just return a default format
        detectedFormat = hintFormat ?? AudioFormat.DvdHq;
        return new MiniAudioDecoder(stream, detectedFormat.Format, detectedFormat.Channels, detectedFormat.SampleRate);
    }

    public ISoundEncoder? CreateEncoder(Stream stream, string formatId, AudioFormat format)
        => formatId == "wav" ? new MiniAudioEncoder(stream, format.Format, format.Channels, format.SampleRate) : null; // MiniAudio's encoder only supports WAV
}