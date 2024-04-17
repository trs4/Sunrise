namespace Sunrise.Model.TagLib.Mpeg;

public struct VideoHeader : IVideoCodec
{
    private static readonly double[] frame_rates = [ 0, 24000d/1001d, 24, 25, 30000d/1001d, 30, 50, 60000d/1001d, 60 ];

    private readonly int _frameRateIndex;

    public VideoHeader(TagLib.File file, long position)
    {
        ArgumentNullException.ThrowIfNull(file);
        file.Seek(position);
        ByteVector data = file.ReadBlock(7);

        if (data.Count < 7)
            throw new CorruptFileException("Insufficient data in header");

        VideoWidth = data.Mid(0, 2).ToUShort() >> 4;
        VideoHeight = data.Mid(1, 2).ToUShort() & 0x0FFF;
        _frameRateIndex = data[3] & 0x0F;
        VideoBitrate = (int)((data.Mid(4, 3).ToUInt() >> 6) & 0x3FFFF);
    }

    public readonly TimeSpan Duration => TimeSpan.Zero;

    public readonly MediaTypes MediaTypes => MediaTypes.Video;

    public readonly string Description => "MPEG Video";

    public int VideoWidth { get; private set; }

    public int VideoHeight { get; private set; }

    public readonly double VideoFrameRate => _frameRateIndex < 9 ? frame_rates[_frameRateIndex] : 0;

    public int VideoBitrate { get; private set; }
}
