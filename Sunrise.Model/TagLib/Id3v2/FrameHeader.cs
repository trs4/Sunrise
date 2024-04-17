namespace Sunrise.Model.TagLib.Id3v2;

public struct FrameHeader
{
    private ReadOnlyByteVector? _frameId;
    private FrameFlags _flags;

    public FrameHeader(ByteVector data, byte version)
    {
        ArgumentNullException.ThrowIfNull(data);
        _flags = 0;
        FrameSize = 0;

        if (version < 2 || version > 4)
            throw new CorruptFileException("Unsupported tag version");

        if (data.Count < (version == 2 ? 3 : 4))
            throw new CorruptFileException("Data must contain at least a frame ID");

        switch (version)
        {
            case 2:
                _frameId = ConvertId(data.Mid(0, 3), version, false);

                if (data.Count < 6)
                    return;

                FrameSize = data.Mid(3, 3).ToUInt();
                return;

            case 3:
                _frameId = ConvertId(data.Mid(0, 4), version, false);

                if (data.Count < 10)
                    return;

                FrameSize = data.Mid(4, 4).ToUInt();
                _flags = (FrameFlags)(((data[8] << 7) & 0x7000) | ((data[9] >> 4) & 0x000C) | ((data[9] << 1) & 0x0040));
                return;

            case 4:
                _frameId = new ReadOnlyByteVector(data.Mid(0, 4));

                if (data.Count < 10)
                    return;

                FrameSize = SynchData.ToUInt(data.Mid(4, 4));
                _flags = (FrameFlags)data.Mid(8, 2).ToUShort();
                return;
            default:
                throw new CorruptFileException("Unsupported tag version");
        }
    }

    public ReadOnlyByteVector? FrameId
    {
        readonly get => _frameId;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _frameId = value.Count == 4 ? value : new ReadOnlyByteVector(value.Mid(0, 4));
        }
    }

    public uint FrameSize { get; set; }

    public FrameFlags Flags
    {
        readonly get => _flags;
        set
        {
            if ((value & (FrameFlags.Compression | FrameFlags.Encryption)) != 0)
                throw new ArgumentException("Encryption and compression are not supported", nameof(value));

            _flags = value;
        }
    }

    public readonly ByteVector Render(byte version)
    {
        var data = new ByteVector();
        ByteVector id = ConvertId(_frameId, version, true) ?? throw new NotImplementedException();

        switch (version)
        {
            case 2:
                data.Add(id);
                data.Add(ByteVector.FromUInt(FrameSize).Mid(1, 3));
                return data;
            case 3:
                ushort new_flags = (ushort)((((ushort)_flags << 1) & 0xE000) | (((ushort)_flags << 4) & 0x00C0) | (((ushort)_flags >> 1) & 0x0020));
                data.Add(id);
                data.Add(ByteVector.FromUInt(FrameSize));
                data.Add(ByteVector.FromUShort(new_flags));
                return data;
            case 4:
                data.Add(id);
                data.Add(SynchData.FromUInt(FrameSize));
                data.Add(ByteVector.FromUShort((ushort)_flags));
                return data;
            default:
                throw new NotImplementedException("Unsupported tag version");
        }
    }

    public static uint Size(byte version) => (uint)(version < 3 ? 6 : 10);

    private static ReadOnlyByteVector? ConvertId(ByteVector id, byte version, bool toVersion)
    {
        if (version >= 4)
            return id as ReadOnlyByteVector ?? new ReadOnlyByteVector(id);

        if (id is null || version < 2)
            return null;

        if (!toVersion && (id == FrameType.EQUA || id == FrameType.RVAD || id == FrameType.TRDA || id == FrameType.TSIZ))
            return null;

        if (version == 2)
        {
            for (int i = 0; i < _version2Frames.GetLength(0); i++)
            {
                if (!_version2Frames[i, toVersion ? 1 : 0].Equals(id))
                    continue;

                return _version2Frames[i, toVersion ? 0 : 1];
            }
        }

        if (version == 3)
        {
            for (int i = 0; i < _version3Frames.GetLength(0); i++)
            {
                if (!_version3Frames[i, toVersion ? 1 : 0].Equals(id))
                    continue;

                return _version3Frames[i, toVersion ? 0 : 1];
            }
        }

        if ((id.Count != 4 && version > 2) || (id.Count != 3 && version == 2))
            return null;

        return id is ReadOnlyByteVector ? id as ReadOnlyByteVector : new ReadOnlyByteVector(id);
    }

    private static readonly ReadOnlyByteVector[,] _version2Frames = new ReadOnlyByteVector[59, 2]
    {
        { "BUF", "RBUF" },
        { "CNT", "PCNT" },
        { "COM", "COMM" },
        { "CRA", "AENC" },
        { "ETC", "ETCO" },
        { "GEO", "GEOB" },
        { "IPL", "TIPL" },
        { "MCI", "MCDI" },
        { "MLL", "MLLT" },
        { "PIC", "APIC" },
        { "POP", "POPM" },
        { "REV", "RVRB" },
        { "SLT", "SYLT" },
        { "STC", "SYTC" },
        { "TAL", "TALB" },
        { "TBP", "TBPM" },
        { "TCM", "TCOM" },
        { "TCO", "TCON" },
        { "TCP", "TCMP" },
        { "TCR", "TCOP" },
        { "TDA", "TDAT" },
        { "TIM", "TIME" },
        { "TDY", "TDLY" },
        { "TEN", "TENC" },
        { "TFT", "TFLT" },
        { "TKE", "TKEY" },
        { "TLA", "TLAN" },
        { "TLE", "TLEN" },
        { "TMT", "TMED" },
        { "TOA", "TOAL" },
        { "TOF", "TOFN" },
        { "TOL", "TOLY" },
        { "TOR", "TDOR" },
        { "TOT", "TOAL" },
        { "TP1", "TPE1" },
        { "TP2", "TPE2" },
        { "TP3", "TPE3" },
        { "TP4", "TPE4" },
        { "TPA", "TPOS" },
        { "TPB", "TPUB" },
        { "TRC", "TSRC" },
        { "TRK", "TRCK" },
        { "TSS", "TSSE" },
        { "TT1", "TIT1" },
        { "TT2", "TIT2" },
        { "TT3", "TIT3" },
        { "TXT", "TOLY" },
        { "TXX", "TXXX" },
        { "TYE", "TDRC" },
        { "UFI", "UFID" },
        { "ULT", "USLT" },
        { "WAF", "WOAF" },
        { "WAR", "WOAR" },
        { "WAS", "WOAS" },
        { "WCM", "WCOM" },
        { "WCP", "WCOP" },
        { "WPB", "WPUB" },
        { "WXX", "WXXX" },
        { "XRV", "RVA2" },
    };

    private static readonly ReadOnlyByteVector[,] _version3Frames = new ReadOnlyByteVector[3, 2]
    {
        { "TORY", "TDOR" },
        { "TYER", "TDRC" },
        { "XRVA", "RVA2" },
    };
}
