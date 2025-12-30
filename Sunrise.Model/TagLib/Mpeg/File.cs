namespace Sunrise.Model.TagLib.Mpeg;

[SupportedMimeType("taglib/mpg", "mpg", FileType.Video)]
[SupportedMimeType("taglib/mpeg", "mpeg", FileType.Video)]
[SupportedMimeType("taglib/mpe", "mpe", FileType.Video)]
[SupportedMimeType("taglib/mpv2", "mpv2", FileType.Video)]
[SupportedMimeType("taglib/m2v", "m2v", FileType.Video)]
[SupportedMimeType("video/x-mpg", FileType.Video)]
[SupportedMimeType("video/mpeg", FileType.Video)]
public class File : NonContainer.File
{
    private static readonly ByteVector MarkerStart = new byte[] { 0, 0, 1 };

    private Version _version;
    private AudioHeader _audioHeader;
    private VideoHeader _videoHeader;
    private bool _videoFound;
    private bool _audioFound;
    private double? _startTime;
    private double _endTime;

    public File(string path, ReadStyle propertiesStyle) : base(path, propertiesStyle) { }

    public File(string path) : base(path) { }

    public File(IFileAbstraction abstraction, ReadStyle propertiesStyle) : base(abstraction, propertiesStyle) { }

    public File(IFileAbstraction abstraction) : base(abstraction) { }

    public override Tag? GetTag(TagTypes type, bool create)
    {
        Tag t = (Tag as NonContainer.Tag)?.GetTag(type);

        if (t is not null || !create)
            return t;

        return type switch
        {
            TagTypes.Id3v1 => EndTag.AddTag(type, Tag),
            TagTypes.Id3v2 => EndTag.AddTag(type, Tag),
            TagTypes.Ape => EndTag.AddTag(type, Tag),
            _ => null,
        };
    }

    protected override void ReadStart(long start, ReadStyle propertiesStyle)
    {
        if ((propertiesStyle & ReadStyle.Average) == 0)
            return;

        FindMarker(ref start, Marker.SystemSyncPacket);
        ReadSystemFile(start);
    }

    protected override void ReadEnd(long end, ReadStyle propertiesStyle)
    {
        GetTag(TagTypes.Id3v1, true);
        GetTag(TagTypes.Id3v2, true);

        if ((propertiesStyle & ReadStyle.Average) == 0 || _startTime is null)
            return;

        if (end == Length)
            end = 0;

        RFindMarker(ref end, Marker.SystemSyncPacket);
        _endTime = ReadTimestamp(end + 4);
    }

    protected override Properties ReadProperties(long start, long end, ReadStyle propertiesStyle)
    {
        var duration = _startTime is null ? TimeSpan.Zero : TimeSpan.FromSeconds(_endTime - (double)_startTime);
        return new Properties(duration, _videoHeader, _audioHeader);
    }

    protected Marker GetMarker(long position)
    {
        Seek(position);
        ByteVector identifier = ReadBlock(4);

        if (identifier.Count == 4 && identifier.StartsWith(MarkerStart))
            return (Marker)identifier[3];

        throw new CorruptFileException("Invalid marker at position " + position);
    }

    protected Marker FindMarker(ref long position)
    {
        position = Find(MarkerStart, position);

        if (position < 0)
            throw new CorruptFileException("Marker not found");

        return GetMarker(position);
    }

    protected void FindMarker(ref long position, Marker marker)
    {
        var packet = new ByteVector(MarkerStart)
        {
            (byte)marker
        };

        position = Find(packet, position);

        if (position < 0)
            throw new CorruptFileException("Marker not found");
    }

    protected void RFindMarker(ref long position, Marker marker)
    {
        var packet = new ByteVector(MarkerStart)
        {
            (byte)marker
        };

        position = RFind(packet, position);

        if (position < 0)
            throw new CorruptFileException("Marker not found");
    }

    protected void ReadSystemFile(long position)
    {
        int sanity_limit = 100;

        for (int i = 0; i < sanity_limit && (_startTime is null || !_audioFound || !_videoFound); i++)
        {
            Marker marker = FindMarker(ref position);

            switch (marker)
            {
                case Marker.SystemSyncPacket:
                    ReadSystemSyncPacket(ref position);
                    break;
                case Marker.SystemPacket:
                case Marker.PaddingPacket:
                    Seek(position + 4);
                    position += ReadBlock(2).ToUShort() + 6;
                    break;
                case Marker.VideoPacket:
                    ReadVideoPacket(ref position);
                    break;
                case Marker.AudioPacket:
                    ReadAudioPacket(ref position);
                    break;
                case Marker.EndOfStream:
                    return;
                default:
                    position += 4;
                    break;
            }
        }
    }

    private void ReadAudioPacket(ref long position)
    {
        Seek(position + 4);
        int length = ReadBlock(2).ToUShort();

        if (!_audioFound)
        {
            ByteVector packetHeaderBytes = ReadBlock(19);
            int i = 0;

            while (i < packetHeaderBytes.Count && packetHeaderBytes[i] == 0xFF)
                i++;

            if ((packetHeaderBytes[i] & 0x40) != 0)
                i++;

            byte timestampFlags = packetHeaderBytes[i];
            long dataOffset = 4 + 2 + i + ((timestampFlags & 0x20) > 0 ? 4 : 0) + ((timestampFlags & 0x10) > 0 ? 4 : 0);
            _audioFound = AudioHeader.Find(out _audioHeader, this, position + dataOffset, length - 9);
        }

        position += length;
    }

    private void ReadVideoPacket(ref long position)
    {
        Seek(position + 4);
        int length = ReadBlock(2).ToUShort();
        long offset = position + 6;

        while (!_videoFound && offset < position + length)
        {
            if (FindMarker(ref offset) == Marker.VideoSyncPacket)
            {
                _videoHeader = new VideoHeader(this, offset + 4);
                _videoFound = true;
            }
            else
                offset += 6;
        }

        position += length;
    }

    private void ReadSystemSyncPacket(ref long position)
    {
        int packet_size = 0;
        Seek(position + 4);
        byte version_info = ReadBlock(1)[0];

        if ((version_info & 0xF0) == 0x20)
        {
            _version = Version.Version1;
            packet_size = 12;
        }
        else if ((version_info & 0xC0) == 0x40)
        {
            _version = Version.Version2;
            Seek(position + 13);
            packet_size = 14 + (ReadBlock(1)[0] & 0x07);
        }
        else
            throw new UnsupportedFormatException("Unknown MPEG version.");

        _startTime ??= ReadTimestamp(position + 4);
        position += packet_size;
    }

    private double ReadTimestamp(long position)
    {
        double high;
        uint low;
        Seek(position);

        if (_version == Version.Version1)
        {
            ByteVector data = ReadBlock(5);
            high = (data[0] >> 3) & 0x01;

            low = ((uint)((data[0] >> 1) & 0x03) << 30) |
                (uint)(data[1] << 22) |
                (uint)((data[2] >> 1) << 15) |
                (uint)(data[3] << 7) |
                (uint)(data[4] >> 1);
        }
        else
        {
            ByteVector data = ReadBlock(6);
            high = (data[0] & 0x20) >> 5;

            low = ((uint)((data[0] & 0x18) >> 3) << 30) |
                (uint)((data[0] & 0x03) << 28) |
                (uint)(data[1] << 20) |
                (uint)((data[2] & 0xF8) << 12) |
                (uint)((data[2] & 0x03) << 13) |
                (uint)(data[3] << 5) |
                (uint)(data[4] >> 3);
        }

        return (((high * 0x10000) * 0x10000) + low) / 90000.0;
    }

}
