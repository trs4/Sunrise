namespace Sunrise.Model.TagLib.Id3v2;

public class UrlLinkFrame : Frame
{
    private StringType _encoding = StringType.Latin1;
    private string[] _textFields = [];
    private ByteVector? _rawData;
    private byte _rawVersion;

    public UrlLinkFrame(ByteVector ident) : base(ident, 4) { }

    public UrlLinkFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal UrlLinkFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public virtual string?[] Text
    {
        get
        {
            ParseRawData();
            return (string[])_textFields.Clone();
        }
        set
        {
            _rawData = null;
            _textFields = value is not null ? (string[])value.Clone() : [];
        }
    }

    public StringType TextEncoding
    {
        get
        {
            ParseRawData();
            return _encoding;
        }
        set => _encoding = value;
    }

    public override string ToString()
    {
        ParseRawData();
        return string.Join("; ", Text);
    }

    public static UrlLinkFrame? Get(Tag tag, ByteVector ident, bool create)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(ident);

        if (ident.Count != 4)
            throw new ArgumentException("Identifier must be four bytes long", nameof(ident));

        foreach (var frame in tag.GetFrames<UrlLinkFrame>(ident))
            return frame;

        if (!create)
            return null;

        var new_frame = new UrlLinkFrame(ident);
        tag.AddFrame(new_frame);
        return new_frame;
    }

    protected override void ParseFields(ByteVector data, byte version)
    {
        _rawData = data;
        _rawVersion = version;
    }

    protected void ParseRawData()
    {
        if (_rawData is null)
            return;

        ByteVector data = _rawData;
        _rawData = null;
        var field_list = new List<string>();
        ByteVector delim = ByteVector.TextDelimiter(_encoding);

        if (FrameId != FrameType.WXXX)
            field_list.AddRange(data.ToStrings(StringType.Latin1, 0));
        else if (data.Count > 1 && !data.Mid(0, delim.Count).Equals(delim))
        {
            string value = data.ToString(StringType.Latin1, 1, data.Count - 1);

            if (value.Length > 1 && value[^1] == 0)
            {
                for (int i = value.Length - 1; i >= 0; i--)
                {
                    if (value[i] != 0)
                    {
                        value = value.Substring(0, i + 1);
                        break;
                    }
                }
            }

            field_list.Add(value);
        }

        while (field_list.Count != 0 && string.IsNullOrEmpty(field_list[^1]))
            field_list.RemoveAt(field_list.Count - 1);

        _textFields = [.. field_list];
    }

    protected override ByteVector RenderFields(byte version)
    {
        if (_rawData is not null && _rawVersion == version)
            return _rawData;

        StringType encoding = CorrectEncoding(TextEncoding, version);
        bool wxxx = FrameId == FrameType.WXXX;
        ByteVector v = wxxx ? new ByteVector((byte)encoding) : [];
        string?[] text = _textFields;

        if (version > 3 || wxxx)
        {
            if (wxxx)
            {
                if (text.Length == 0)
                    text = [null, null];
                else if (text.Length == 1)
                    text = [text[0], null];
            }

            v.Add(ByteVector.FromString(string.Join("/", text), StringType.Latin1));
        }
        else
            v.Add(ByteVector.FromString(string.Join("/", text), StringType.Latin1));

        return v;
    }

    public override Frame Clone()
    {
        UrlLinkFrame frame = (this is UserUrlLinkFrame) ? new UserUrlLinkFrame(null, _encoding) : new UrlLinkFrame(FrameId);
        frame._textFields = (string[])_textFields.Clone();

        if (_rawData is not null)
            frame._rawData = new ByteVector(_rawData);

        frame._rawVersion = _rawVersion;
        return frame;
    }

}
