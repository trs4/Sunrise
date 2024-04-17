namespace Sunrise.Model.TagLib.Id3v2;

public class SynchronisedLyricsFrame : Frame
{
    private string _language;
    private SynchedText[] _text = [];

    public SynchronisedLyricsFrame(string description, string language, SynchedTextType type, StringType encoding)
        : base(FrameType.SYLT, 4)
    {
        TextEncoding = encoding;
        _language = language;
        Description = description;
        Type = type;
    }

    public SynchronisedLyricsFrame(string description, string language, SynchedTextType type) : this(description, language, type, Tag.DefaultEncoding) { }

    public SynchronisedLyricsFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal SynchronisedLyricsFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public StringType TextEncoding { get; set; } = Tag.DefaultEncoding;

    public string Language
    {
        get => (_language is not null && _language.Length > 2) ? _language.Substring(0, 3) : "XXX";
        set => _language = value;
    }

    public string Description { get; set; }

    public TimestampFormat Format { get; set; } = TimestampFormat.Unknown;

    public SynchedTextType Type { get; set; } = SynchedTextType.Other;

    public SynchedText[] Text
    {
        get => _text;
        set => _text = value ?? [];
    }

    public static SynchronisedLyricsFrame? Get(Tag tag, string description, string language, SynchedTextType type, bool create)
    {
        foreach (Frame f in tag)
        {
            if (f is not SynchronisedLyricsFrame lyr)
                continue;

            if (lyr.Description == description && (language is null || language == lyr.Language) && type == lyr.Type)
                return lyr;
        }

        if (!create)
            return null;

        var frame = new SynchronisedLyricsFrame(description, language, type);
        tag.AddFrame(frame);
        return frame;
    }

    public static SynchronisedLyricsFrame? GetPreferred(Tag tag, string description, string language, SynchedTextType type)
    {
        int best_value = -1;
        SynchronisedLyricsFrame best_frame = null;

        foreach (Frame f in tag)
        {
            if (f is not SynchronisedLyricsFrame cf)
                continue;

            int value = 0;

            if (cf.Language == language)
                value += 4;

            if (cf.Description == description)
                value += 2;

            if (cf.Type == type)
                value += 1;

            if (value == 7)
                return cf;

            if (value <= best_value)
                continue;

            best_value = value;
            best_frame = cf;
        }

        return best_frame;
    }

    protected override void ParseFields(ByteVector data, byte version)
    {
        if (data.Count < 6)
            throw new CorruptFileException("Not enough bytes in field");

        TextEncoding = (StringType)data[0];
        _language = data.ToString(StringType.Latin1, 1, 3);
        Format = (TimestampFormat)data[4];
        Type = (SynchedTextType)data[5];

        var delim = ByteVector.TextDelimiter(TextEncoding);
        int delim_index = data.Find(delim, 6, delim.Count);

        if (delim_index < 0)
            throw new CorruptFileException("Text delimiter expected");

        Description = data.ToString(TextEncoding, 6, delim_index - 6);
        int offset = delim_index + delim.Count;
        var l = new List<SynchedText>();

        while (offset + delim.Count + 4 < data.Count)
        {
            delim_index = data.Find(delim, offset, delim.Count);

            if (delim_index < offset)
                throw new CorruptFileException("Text delimiter expected");

            string text = data.ToString(TextEncoding, offset, delim_index - offset);
            offset = delim_index + delim.Count;

            if (offset + 4 > data.Count)
                break;

            l.Add(new SynchedText(data.Mid(offset, 4).ToUInt(), text));
            offset += 4;
        }

        _text = [.. l];
    }

    protected override ByteVector RenderFields(byte version)
    {
        var encoding = CorrectEncoding(TextEncoding, version);
        var delim = ByteVector.TextDelimiter(encoding);

        var v = new ByteVector
        {
            (byte)encoding,
            ByteVector.FromString (Language, StringType.Latin1),
            (byte)Format,
            (byte)Type,
            ByteVector.FromString (Description, encoding),
            delim,
        };

        foreach (SynchedText t in _text)
        {
            v.Add(ByteVector.FromString(t.Text, encoding));
            v.Add(delim);
            v.Add(ByteVector.FromUInt((uint)t.Time));
        }

        return v;
    }

    public override Frame Clone() => new SynchronisedLyricsFrame(Description, _language, Type, TextEncoding)
    {
        Format = Format,
        _text = (SynchedText[])_text.Clone(),
    };
}
