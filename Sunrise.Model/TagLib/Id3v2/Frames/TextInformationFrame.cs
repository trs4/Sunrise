using System.Globalization;
using System.Text;

namespace Sunrise.Model.TagLib.Id3v2;

public class TextInformationFrame : Frame
{
    private StringType _encoding = Tag.DefaultEncoding;
    private string[] _textFields = [];
    private ByteVector? _rawData;
    private byte _rawVersion;
    private StringType _rawEncoding = StringType.Latin1;

    public TextInformationFrame(ByteVector ident, StringType encoding)
        : base(ident, 4)
        => _encoding = encoding;

    public TextInformationFrame(ByteVector ident) : this(ident, Tag.DefaultEncoding) { }

    public TextInformationFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal TextInformationFrame(ByteVector data, int offset, FrameHeader header, byte version)
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

    public override ByteVector Render(byte version)
    {
        if (version != 3 || FrameId != FrameType.TDRC)
            return base.Render(version);

        string text = ToString();

        if (text.Length < 10 || text[4] != '-' || text[7] != '-')
            return base.Render(version);

        ByteVector output = [];
        TextInformationFrame f;

        f = new TextInformationFrame(FrameType.TYER, _encoding)
        {
            Text = [text.Substring(0, 4)]
        };

        output.Add(f.Render(version));

        f = new TextInformationFrame(FrameType.TDAT, _encoding)
        {
            Text =
            [
                string.Concat(text.AsSpan(5, 2), text.AsSpan(8, 2)),
            ],
        };

        output.Add(f.Render(version));

        if (text.Length < 16 || text[10] != 'T' || text[13] != ':')
            return output;

        f = new TextInformationFrame(FrameType.TIME, _encoding)
        {
            Text =
            [
                string.Concat(text.AsSpan(11, 2), text.AsSpan(14, 2)),
            ],
        };

        output.Add(f.Render(version));
        return output;
    }

    public static TextInformationFrame? Get(Tag tag, ByteVector ident, StringType encoding, bool create)
    {
        ArgumentNullException.ThrowIfNull(tag);
        ArgumentNullException.ThrowIfNull(ident);

        if (ident.Count != 4)
            throw new ArgumentException("Identifier must be four bytes long.", nameof(ident));

        foreach (var frame in tag.GetFrames<TextInformationFrame>(ident))
            return frame;

        if (!create)
            return null;

        var new_frame = new TextInformationFrame(ident, encoding);
        tag.AddFrame(new_frame);
        return new_frame;
    }

    public static TextInformationFrame? Get(Tag tag, ByteVector ident, bool create) => Get(tag, ident, Tag.DefaultEncoding, create);

    protected override void ParseFields(ByteVector data, byte version)
    {
        _rawData = data;
        _rawVersion = version;
        _rawEncoding = (StringType)data[0];
    }

    protected void ParseRawData()
    {
        if (_rawData is null)
            return;

        ByteVector data = _rawData;
        _rawData = null;
        _encoding = (StringType)data[0];
        List<string> field_list = [];
        ByteVector delim = ByteVector.TextDelimiter(_encoding);

        if (_rawVersion > 3 || FrameId == FrameType.TXXX)
            field_list.AddRange(data.ToStrings(_encoding, 1));
        else if (data.Count > 1 && !data.Mid(1, delim.Count).Equals(delim))
        {
            string value = data.ToString(_encoding, 1, data.Count - 1);
            int null_index = value.IndexOf('\x00');

            if (null_index >= 0)
                value = value.Substring(0, null_index);

            if (FrameId == FrameType.TCOM ||
                FrameId == FrameType.TEXT ||
                FrameId == FrameType.TMCL ||
                FrameId == FrameType.TOLY ||
                FrameId == FrameType.TOPE ||
                FrameId == FrameType.TSOC ||
                FrameId == FrameType.TSOP ||
                FrameId == FrameType.TSO2 ||
                FrameId == FrameType.TPE1 ||
                FrameId == FrameType.TPE2 ||
                FrameId == FrameType.TPE3 ||
                FrameId == FrameType.TPE4)
            {
                field_list.AddRange(value.Split('/'));
            }
            else if (FrameId == FrameType.TCON)
            {
                while (value.Length > 1 && value[0] == '(')
                {
                    int closing = value.IndexOf(')');

                    if (closing < 0)
                        break;

                    string number = value.Substring(1, closing - 1);
                    field_list.Add(number);
                    value = value.Substring(closing + 1).TrimStart('/', ' ');
                    string text = Genres.IndexToAudio(number);

                    if (text != null && value.StartsWith(text))
                        value = value.Substring(text.Length).TrimStart('/', ' ');
                }

                if (value.Length > 0)
                    field_list.AddRange(value.Split(['/', ';']));
            }
            else
                field_list.Add(value);
        }

        while (field_list.Count != 0 && string.IsNullOrEmpty(field_list[^1]))
            field_list.RemoveAt(field_list.Count - 1);

        _textFields = [.. field_list];
    }

    protected override ByteVector RenderFields(byte version)
    {
        if (_rawData is not null && _rawVersion == version && _rawEncoding == Tag.DefaultEncoding)
            return _rawData;

        StringType encoding = CorrectEncoding(TextEncoding, version);
        ByteVector v = new ByteVector((byte)encoding);
        string?[] text = _textFields;
        bool txxx = FrameId == FrameType.TXXX;

        if (version > 3 || txxx)
        {

            if (txxx)
            {
                if (text.Length == 0)
                    text = [null, null];
                else if (text.Length == 1)
                    text = [text[0], null];
            }

            for (int i = 0; i < text.Length; i++)
            {
                if (i != 0)
                    v.Add(ByteVector.TextDelimiter(encoding));

                if (text[i] is not null)
                    v.Add(ByteVector.FromString(text[i], encoding));
            }
        }
        else if (FrameId == FrameType.TCON)
        {
            bool prev_value_indexed = true;
            var data = new StringBuilder();

            foreach (string s in text)
            {
                if (!prev_value_indexed)
                {
                    data.Append(';').Append(s);
                    continue;
                }

                if (prev_value_indexed = byte.TryParse(s, out var id))
                    data.AppendFormat(CultureInfo.InvariantCulture, "({0})", id);
                else
                    data.Append(s);
            }

            v.Add(ByteVector.FromString(data.ToString(), encoding));
        }
        else
            v.Add(ByteVector.FromString(string.Join("/", text), encoding));

        return v;
    }

    public override Frame Clone()
    {
        var frame = (this is UserTextInformationFrame) ? new UserTextInformationFrame(null, _encoding) : new TextInformationFrame(FrameId, _encoding);
        frame._textFields = (string[])_textFields.Clone();

        if (_rawData is not null)
            frame._rawData = new ByteVector(_rawData);

        frame._rawVersion = _rawVersion;
        return frame;
    }

}
