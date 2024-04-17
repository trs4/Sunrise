using System.Text;

namespace Sunrise.Model.TagLib;

public class Properties : IAudioCodec, IVideoCodec, IPhotoCodec
{
    private readonly ICodec[] _codecs = [];
    private readonly TimeSpan _duration = TimeSpan.Zero;

    public Properties() { }

    public Properties(TimeSpan duration, params ICodec[] codecs)
    {
        _duration = duration;

        if (codecs is not null)
            _codecs = codecs;
    }

    public Properties(TimeSpan duration, IEnumerable<ICodec> codecs)
    {
        _duration = duration;

        if (codecs is not null)
            _codecs = new List<ICodec>(codecs).ToArray();
    }

    public IEnumerable<ICodec> Codecs => _codecs;

    public TimeSpan Duration
    {
        get
        {
            var duration = _duration;

            if (duration != TimeSpan.Zero)
                return duration;

            foreach (ICodec codec in _codecs)
            {
                if (codec is not null && codec.Duration > duration)
                    duration = codec.Duration;
            }

            return duration;
        }
    }

    public MediaTypes MediaTypes
    {
        get
        {
            var types = MediaTypes.None;

            foreach (ICodec codec in _codecs)
            {
                if (codec is not null)
                    types |= codec.MediaTypes;
            }

            return types;
        }
    }

    public string Description
    {
        get
        {
            var builder = new StringBuilder();

            foreach (ICodec codec in _codecs)
            {
                if (codec is null)
                    continue;

                if (builder.Length != 0)
                    builder.Append("; ");

                builder.Append(codec.Description);
            }

            return builder.ToString();
        }
    }

    public int AudioBitrate
    {
        get
        {
            foreach (ICodec codec in _codecs)
            {
                if (codec is null || (codec.MediaTypes & MediaTypes.Audio) == 0)
                    continue;

                if (codec is IAudioCodec audio && audio.AudioBitrate != 0)
                    return audio.AudioBitrate;
            }

            return 0;
        }
    }

    public int AudioSampleRate
    {
        get
        {
            foreach (ICodec codec in _codecs)
            {
                if (codec is null || (codec.MediaTypes & MediaTypes.Audio) == 0)
                    continue;

                if (codec is IAudioCodec audio && audio.AudioSampleRate != 0)
                    return audio.AudioSampleRate;
            }

            return 0;
        }
    }

    public int BitsPerSample
    {
        get
        {
            foreach (ICodec codec in _codecs)
            {
                if (codec is null || (codec.MediaTypes & MediaTypes.Audio) == 0)
                    continue;

                if (codec is ILosslessAudioCodec lossless && lossless.BitsPerSample != 0)
                    return lossless.BitsPerSample;
            }

            return 0;
        }
    }

    public int AudioChannels
    {
        get
        {
            foreach (ICodec codec in _codecs)
            {
                if (codec is null || (codec.MediaTypes & MediaTypes.Audio) == 0)
                    continue;

                if (codec is IAudioCodec audio && audio.AudioChannels != 0)
                    return audio.AudioChannels;
            }

            return 0;
        }
    }

    public int VideoWidth
    {
        get
        {
            foreach (ICodec codec in _codecs)
            {
                if (codec is null || (codec.MediaTypes & MediaTypes.Video) == 0)
                    continue;

                if (codec is IVideoCodec video && video.VideoWidth != 0)
                    return video.VideoWidth;
            }

            return 0;
        }
    }

    public int VideoHeight
    {
        get
        {
            foreach (ICodec codec in _codecs)
            {
                if (codec is null || (codec.MediaTypes & MediaTypes.Video) == 0)
                    continue;

                if (codec is IVideoCodec video && video.VideoHeight != 0)
                    return video.VideoHeight;
            }

            return 0;
        }
    }

    public int PhotoWidth
    {
        get
        {
            foreach (ICodec codec in _codecs)
            {
                if (codec is null || (codec.MediaTypes & MediaTypes.Photo) == 0)
                    continue;

                if (codec is IPhotoCodec photo && photo.PhotoWidth != 0)
                    return photo.PhotoWidth;
            }

            return 0;
        }
    }

    public int PhotoHeight
    {
        get
        {
            foreach (ICodec codec in _codecs)
            {
                if (codec is null || (codec.MediaTypes & MediaTypes.Photo) == 0)
                    continue;

                if (codec is IPhotoCodec photo && photo.PhotoHeight != 0)
                    return photo.PhotoHeight;
            }

            return 0;
        }
    }

    public int PhotoQuality
    {
        get
        {
            foreach (ICodec codec in _codecs)
            {
                if (codec is null || (codec.MediaTypes & MediaTypes.Photo) == 0)
                    continue;

                if (codec is IPhotoCodec photo && photo.PhotoQuality != 0)
                    return photo.PhotoQuality;
            }

            return 0;
        }
    }

}
