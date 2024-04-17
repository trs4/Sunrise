namespace Sunrise.Model.TagLib;

public interface IAudioCodec : ICodec
{
    int AudioBitrate { get; }

    int AudioSampleRate { get; }

    int AudioChannels { get; }
}
