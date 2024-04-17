using System.Text;

namespace Sunrise.Model.TagLib.Mpeg;

public struct AudioHeader : IAudioCodec
{
    private static readonly int[,] _sampleRates = new int[3, 4]
    {
        { 44100, 48000, 32000, 0 }, // Version 1
        { 22050, 24000, 16000, 0 }, // Version 2
        { 11025, 12000,  8000, 0 }  // Version 2.5
    };

    private static readonly int[,] _blockSize = new int[3, 4]
    {
        { 0, 384, 1152, 1152 }, // Version 1
        { 0, 384, 1152,  576 }, // Version 2
        { 0, 384, 1152,  576 }  // Version 2.5
    };

    private static readonly int[,,] _bitrates = new int[2, 3, 16]
    {
        { // Version 1
            { 0, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448, -1 }, // layer 1
            { 0, 32, 48, 56,  64,  80,  96, 112, 128, 160, 192, 224, 256, 320, 384, -1 }, // layer 2
            { 0, 32, 40, 48,  56,  64,  80,  96, 112, 128, 160, 192, 224, 256, 320, -1 },  // layer 3
        },
        { // Version 2 or 2.5
            { 0, 32, 48, 56, 64, 80, 96, 112, 128, 144, 160, 176, 192, 224, 256, -1 }, // layer 1
            { 0,  8, 16, 24, 32, 40, 48,  56,  64,  80,  96, 112, 128, 144, 160, -1 }, // layer 2
            { 0,  8, 16, 24, 32, 40, 48,  56,  64,  80,  96, 112, 128, 144, 160, -1 }  // layer 3
        },
    };

    private readonly uint _flags;
    private long _streamLength;
    private XingHeader _xingHeader;
    private TimeSpan _duration;

    public static readonly AudioHeader Unknown = new AudioHeader(0, 0, XingHeader.Unknown, VBRIHeader.Unknown);

    private AudioHeader(uint flags, long streamLength, XingHeader xingHeader, VBRIHeader vbriHeader)
    {
        _flags = flags;
        _streamLength = streamLength;
        _xingHeader = xingHeader;
        VBRIHeader = vbriHeader;
        _duration = TimeSpan.Zero;
    }

    private AudioHeader(ByteVector data, TagLib.File file, long position)
    {
        _duration = TimeSpan.Zero;
        _streamLength = 0;
        string error = GetHeaderError(data);

        if (error is not null)
            throw new CorruptFileException(error);

        _flags = data.ToUInt();
        _xingHeader = XingHeader.Unknown;
        VBRIHeader = VBRIHeader.Unknown;
        file.Seek(position + XingHeader.XingHeaderOffset(Version, ChannelMode));
        ByteVector xing_data = file.ReadBlock(16);

        if (xing_data.Count == 16 && xing_data.StartsWith(XingHeader.FileIdentifier))
            _xingHeader = new XingHeader(xing_data);

        if (_xingHeader.Present)
            return;

        file.Seek(position + VBRIHeader.VBRIHeaderOffset());
        ByteVector vbri_data = file.ReadBlock(24);

        if (vbri_data.Count == 24 && vbri_data.StartsWith(VBRIHeader.FileIdentifier))
            VBRIHeader = new VBRIHeader(vbri_data);
    }

    public readonly Version Version => ((_flags >> 19) & 0x03) switch
    {
        0 => Version.Version25,
        2 => Version.Version2,
        _ => Version.Version1,
    };

    public readonly int AudioLayer => ((_flags >> 17) & 0x03) switch
    {
        1 => 3,
        2 => 2,
        _ => 1,
    };

    public int AudioBitrate
    {
        get
        {
            if (_xingHeader.TotalSize > 0 && _xingHeader.TotalFrames > 0 && Duration > TimeSpan.Zero)
                return (int)Math.Round((((XingHeader.TotalSize * 8L) / Duration.TotalSeconds) / 1000.0));

            if (VBRIHeader.TotalSize > 0 && VBRIHeader.TotalFrames > 0 && Duration > TimeSpan.Zero)
                return (int)Math.Round((((VBRIHeader.TotalSize * 8L) / Duration.TotalSeconds) / 1000.0));

            return _bitrates[
                Version == Version.Version1 ? 0 : 1,
                AudioLayer > 0 ? AudioLayer - 1 : 0,
                (int)(_flags >> 12) & 0x0F];
        }
    }

    public readonly int AudioSampleRate => _sampleRates[(int)Version, (int)(_flags >> 10) & 0x03];

    public int AudioChannels => ChannelMode == ChannelMode.SingleChannel ? 1 : 2;

    public int AudioFrameLength
    {
        get
        {
            switch (AudioLayer)
            {
                case 1:
                    return 48000 * AudioBitrate / AudioSampleRate + (IsPadded ? 4 : 0);
                case 2:
                    return 144000 * AudioBitrate / AudioSampleRate + (IsPadded ? 1 : 0);
                case 3:
                    if (Version == Version.Version1)
                        goto case 2;

                    return 72000 * AudioBitrate / AudioSampleRate + (IsPadded ? 1 : 0);
                default:
                    return 0;
            }
        }
    }

    public TimeSpan Duration
    {
        get
        {
            if (_duration > TimeSpan.Zero)
                return _duration;

            if (_xingHeader.TotalFrames > 0)
            {
                double time_per_frame = _blockSize[(int)Version, AudioLayer] / (double)AudioSampleRate;
                _duration = TimeSpan.FromSeconds(time_per_frame * XingHeader.TotalFrames);
            }
            else if (VBRIHeader.TotalFrames > 0)
            {
                double time_per_frame = _blockSize[(int)Version, AudioLayer] / (double)AudioSampleRate;
                _duration = TimeSpan.FromSeconds(Math.Round(time_per_frame * VBRIHeader.TotalFrames));
            }
            else if (AudioFrameLength > 0 && AudioBitrate > 0)
            {
                int frames = (int)((_streamLength + AudioFrameLength - 1) / AudioFrameLength);
                _duration = TimeSpan.FromSeconds((double)(AudioFrameLength * frames) / (AudioBitrate * 125));
            }

            return _duration;
        }
    }

    public readonly string Description
    {
        get
        {
            var builder = new StringBuilder();
            builder.Append("MPEG Version ");

            switch (Version)
            {
                case Version.Version1:
                    builder.Append("1");
                    break;
                case Version.Version2:
                    builder.Append("2");
                    break;
                case Version.Version25:
                    builder.Append("2.5");
                    break;
            }

            builder.Append(" Audio, Layer ");
            builder.Append(AudioLayer);

            if (_xingHeader.Present || VBRIHeader.Present)
                builder.Append(" VBR");

            return builder.ToString();
        }
    }

    public readonly MediaTypes MediaTypes => MediaTypes.Audio;

    public readonly bool IsProtected => ((_flags >> 16) & 1) == 0;

    public readonly bool IsPadded => ((_flags >> 9) & 1) == 1;

    public readonly bool IsCopyrighted => ((_flags >> 3) & 1) == 1;

    public readonly bool IsOriginal => ((_flags >> 2) & 1) == 1;

    public readonly ChannelMode ChannelMode => (ChannelMode)((_flags >> 6) & 0x03);

    public readonly XingHeader XingHeader => _xingHeader;

    public VBRIHeader VBRIHeader { get; }

    public void SetStreamLength(long streamLength)
    {
        _streamLength = streamLength;

        if (_xingHeader.TotalFrames == 0 || VBRIHeader.TotalFrames == 0)
            _duration = TimeSpan.Zero;
    }

    public static bool Find(out AudioHeader header, TagLib.File file, long position, int length)
    {
        ArgumentNullException.ThrowIfNull(file);
        long end = position + length;
        header = Unknown;
        file.Seek(position);
        ByteVector buffer = file.ReadBlock(3);

        if (buffer.Count < 3)
            return false;

        do
        {
            file.Seek(position + 3);
            buffer = buffer.Mid(buffer.Count - 3);
            buffer.Add(file.ReadBlock((int)TagLib.File.BufferSize));

            for (int i = 0; i < buffer.Count - 3 && (length < 0 || position + i < end); i++)
            {
                if (buffer[i] == 0xFF && buffer[i + 1] > 0xE0)
                {
                    ByteVector data = buffer.Mid(i, 4);

                    if (GetHeaderError(data) is null)
                    {
                        try
                        {
                            header = new AudioHeader(data, file, position + i);
                            return true;
                        }
                        catch (CorruptFileException) { }
                    }
                }
            }

            position += TagLib.File.BufferSize;
        } while (buffer.Count > 3 && (length < 0 || position < end));

        return false;
    }

    public static bool Find(out AudioHeader header, TagLib.File file, long position) => Find(out header, file, position, -1);

    private static string? GetHeaderError(ByteVector data)
    {
        if (data.Count < 4)
            return "Insufficient header length";

        if (data[0] != 0xFF)
            return "First byte did not match MPEG synch";

        if ((data[1] & 0xE6) <= 0xE0 || (data[1] & 0x18) == 0x08)
            return "Second byte did not match MPEG synch";

        uint flags = data.ToUInt();

        if (((flags >> 12) & 0x0F) == 0x0F)
            return "Header uses invalid bitrate index";

        if (((flags >> 10) & 0x03) == 0x03)
            return "Invalid sample rate";

        return null;
    }

}
