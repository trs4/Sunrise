namespace Sunrise.Model.TagLib.Id3v2;

public class AttachmentFrame : Frame, IPicture, ILazy
{
    private StringType _encoding = Tag.DefaultEncoding;
    private string _mimeType;
    private PictureType _type = PictureType.Other;
    private string _filename;
    private string? _description;
    private ByteVector _data;
    private ByteVector? _rawData;
    private byte _rawVersion;
    private File.IFileAbstraction? _file;
    private readonly long _streamOffset;
    private readonly long _streamSize = -1;

    public AttachmentFrame() : base(FrameType.APIC, 4) { }

    public AttachmentFrame(IPicture picture)
        : base(FrameType.APIC, 4)
    {
        ArgumentNullException.ThrowIfNull(picture);
        Type = picture.Type;
        _mimeType = picture.MimeType;
        _filename = picture.Filename;
        _description = picture.Description;
        _data = picture.Data;
    }

    public AttachmentFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal AttachmentFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public AttachmentFrame(File.IFileAbstraction abstraction, long offset, long size, FrameHeader header, byte version)
        : base(header)
    {
        ArgumentNullException.ThrowIfNull(abstraction);
        _file = abstraction;
        _streamOffset = offset;
        _streamSize = size;
        _rawVersion = version;
    }

    public StringType TextEncoding
    {
        get
        {
            if (_file is not null)
                Load();

            ParseRawData();
            return _encoding;
        }
        set
        {
            if (_file is not null)
                Load();

            _encoding = value;
        }
    }

    public string MimeType
    {
        get
        {
            if (_file is not null)
                Load();

            ParseRawData();

            if (_mimeType is not null)
                return _mimeType;

            return string.Empty;
        }
        set
        {
            if (_file is not null)
                Load();

            _mimeType = value;
        }
    }

    public PictureType Type
    {
        get
        {
            if (_file is not null)
                Load();

            ParseRawData();
            return _type;
        }
        set
        {
            if (_file is not null)
                Load();

            var frameid = value == PictureType.NotAPicture ? FrameType.GEOB : FrameType.APIC;

            if (_header.FrameId != frameid)
                _header = new FrameHeader(frameid, 4);

            _type = value;
        }
    }

    public string Filename
    {
        get
        {
            if (_file is not null)
                Load();

            return _filename;
        }
        set
        {
            if (_file is not null)
                Load();

            _filename = value;
        }
    }

    public string? Description
    {
        get
        {
            if (_file is not null)
                Load();

            ParseRawData();

            if (_description is not null)
                return _description;

            return string.Empty;
        }
        set
        {
            if (_file != null)
                Load();

            _description = value;
        }
    }

    public ByteVector Data
    {
        get
        {
            if (_file is not null)
                Load();

            ParseRawData();
            return _data is not null ? _data : [];
        }
        set
        {
            if (_file is not null)
                Load();

            _data = value;
        }
    }

    public bool IsLoaded => _data is not null || _rawData is not null;

    public override string ToString()
    {
        if (_file is not null)
            Load();

        var builder = new System.Text.StringBuilder();

        if (string.IsNullOrEmpty(Description))
        {
            builder.Append(Description);
            builder.Append(' ');
        }

        builder.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "[{0}] {1} bytes", MimeType, Data.Count);
        return builder.ToString();
    }

    public static AttachmentFrame? Get(Tag tag, string description, bool create) => Get(tag, description, PictureType.Other, create);

    public static AttachmentFrame? Get(Tag tag, PictureType type, bool create) => Get(tag, null, type, create);

    public static AttachmentFrame? Get(Tag tag, string? description, PictureType type, bool create)
    {
        AttachmentFrame att;

        foreach (Frame frame in tag.GetFrames<AttachmentFrame>())
        {
            att = frame as AttachmentFrame;

            if (att is null)
                continue;

            if (description is not null && att.Description != description)
                continue;

            if (type != PictureType.Other && att.Type != type)
                continue;

            return att;
        }

        if (!create)
            return null;

        att = new AttachmentFrame
        {
            Description = description,
            Type = type
        };

        tag.AddFrame(att);
        return att;
    }

    public void Load()
    {
        if (_file is null)
            return;

        Stream stream = null;
        ByteVector data = null;

        try
        {
            if (_streamSize == 0)
                data = [];
            else if (_streamSize > 0)
            {
                stream = _file.ReadStream;
                stream.Seek(_streamOffset, SeekOrigin.Begin);

                int count = 0, read = 0, needed = (int)_streamSize;
                byte[] buffer = new byte[needed];

                do
                {
                    count = stream.Read(buffer, read, needed);

                    read += count;
                    needed -= count;
                } while (needed > 0 && count != 0);

                data = new ByteVector(buffer, read);
            }
            else
            {
                stream = _file.ReadStream;
                stream.Seek(_streamOffset, SeekOrigin.Begin);
                data = ByteVector.FromStream(stream);
            }
        }
        finally
        {
            if (stream is not null && _file is not null)
                _file.CloseStream(stream);

            _file = null;
        }

        _rawData = FieldData(data, -(int)FrameHeader.Size(_rawVersion), _rawVersion);
        ParseRawData();
    }

    protected override void ParseFields(ByteVector data, byte version)
    {
        if (_file is not null)
            Load();

        if (data.Count < 5)
            throw new CorruptFileException("A picture frame must contain at least 5 bytes.");

        _rawData = data;
        _rawVersion = version;
    }

    protected void ParseRawData()
    {
        if (_file is not null)
            Load();

        if (_rawData is null)
            return;

        _data = _rawData;
        _rawData = null;
        int pos = 0;
        int offset;
        _encoding = (StringType)_data[pos++];
        ByteVector delim = ByteVector.TextDelimiter(_encoding);

        if (_header.FrameId == FrameType.APIC)
        {
            if (_rawVersion > 2) // Retrieve an ID3v2 Attached Picture (APIC)
            {
                offset = _data.Find(ByteVector.TextDelimiter(StringType.Latin1), pos);

                if (offset < pos)
                    return;

                _mimeType = _data.ToString(StringType.Latin1, pos, offset - pos);
                pos = offset + 1;
            }
            else
            {
                ByteVector ext = _data.Mid(pos, 3);
                _mimeType = Picture.GetMimeFromExtension(ext.ToString());
                pos += 3;
            }

            Type = (PictureType)_data[pos++];
            offset = _data.Find(delim, pos, delim.Count);

        }
        else if (_header.FrameId == FrameType.GEOB) // Retrieve an ID3v2 General Encapsulated Object (GEOB)
        {
            offset = _data.Find(ByteVector.TextDelimiter(StringType.Latin1), pos);

            if (offset < pos)
                return;

            _mimeType = _data.ToString(StringType.Latin1, pos, offset - pos);

            pos = offset + 1;
            offset = _data.Find(delim, pos, delim.Count);

            if (offset < pos)
                return;

            _filename = _data.ToString(_encoding, pos, offset - pos);
            pos = offset + delim.Count;
            offset = _data.Find(delim, pos, delim.Count);
            Type = PictureType.NotAPicture;
        }
        else
            throw new InvalidOperationException("Bad Frame type");

        if (offset < pos)
            return;

        _description = _data.ToString(_encoding, pos, offset - pos);
        pos = offset + delim.Count;
        _data.RemoveRange(0, pos);
    }

    protected override ByteVector RenderFields(byte version)
    {
        if (_file is not null)
            Load();

        if (_rawData is not null && _rawVersion == version)
            return _rawData;

        StringType encoding = CorrectEncoding(TextEncoding, version);
        ByteVector data = [];

        if (_header.FrameId == FrameType.APIC) // Make an ID3v2 Attached Picture (APIC)
        {
            data.Add((byte)encoding);

            if (version == 2)
            {
                string ext = Picture.GetExtensionFromMime(MimeType);
                data.Add(ext is not null && ext.Length == 3 ? ext.ToUpper() : "XXX");
            }
            else
            {
                data.Add(ByteVector.FromString(MimeType, StringType.Latin1));
                data.Add(ByteVector.TextDelimiter(StringType.Latin1));
            }

            data.Add((byte)_type);
            data.Add(ByteVector.FromString(Description, encoding));
            data.Add(ByteVector.TextDelimiter(encoding));
        }
        else if (_header.FrameId == FrameType.GEOB) // Make an ID3v2 General Encapsulated Object (GEOB)
        {
            data.Add((byte)encoding);

            if (MimeType is not null)
                data.Add(ByteVector.FromString(MimeType, StringType.Latin1));

            data.Add(ByteVector.TextDelimiter(StringType.Latin1));

            if (_filename is not null)
                data.Add(ByteVector.FromString(_filename, encoding));

            data.Add(ByteVector.TextDelimiter(encoding));

            if (Description is not null)
                data.Add(ByteVector.FromString(Description, encoding));

            data.Add(ByteVector.TextDelimiter(encoding));
        }
        else
            throw new InvalidOperationException("Bad Frame type");

        data.Add(_data);
        return data;
    }

    public override Frame Clone()
    {
        if (_file is not null)
            Load();

        var frame = new AttachmentFrame
        {
            _encoding = _encoding,
            _mimeType = _mimeType,
            Type = _type,
            _filename = _filename,
            _description = _description
        };

        if (_data is not null)
            frame._data = new ByteVector(_data);

        if (_rawData is not null)
            frame._data = new ByteVector(_rawData);

        frame._rawVersion = _rawVersion;
        return frame;
    }

}
