namespace Sunrise.Model.TagLib.Id3v2;

public class UnsynchronisedLyricsFrame : Frame
{
    private string _language;
    private string _description;
    private string _text;

    public UnsynchronisedLyricsFrame(string description, string language, StringType encoding)
        : base(FrameType.USLT, 4)
    {
        TextEncoding = encoding;
        _language = language;
        _description = description;
    }

    public UnsynchronisedLyricsFrame(string description, string? language) : this(description, language, Tag.DefaultEncoding) { }

    public UnsynchronisedLyricsFrame(string description) : this(description, null) { }

    public UnsynchronisedLyricsFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal UnsynchronisedLyricsFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public StringType TextEncoding { get; set; } = Tag.DefaultEncoding;

    public string Language
    {
        get => _language is not null && _language.Length > 2 ? _language.Substring(0, 3) : "XXX";
        set => _language = value;
    }

    public string Description
    {
        get => _description ?? string.Empty;
        set => _description = value;
    }

    public string Text
    {
        get => _text ?? string.Empty;
        set => _text = value;
    }

    public override string ToString() => Text;

    public static UnsynchronisedLyricsFrame? Get(Tag tag, string description, string language, bool create)
    {
        UnsynchronisedLyricsFrame uslt;

        foreach (Frame frame in tag.GetFrames(FrameType.USLT))
        {
            uslt = frame as UnsynchronisedLyricsFrame;

            if (uslt is null)
                continue;

            if (uslt.Description != description)
                continue;

            if (language is not null && language != uslt.Language)
                continue;

            return uslt;
        }

        if (!create)
            return null;

        uslt = new UnsynchronisedLyricsFrame(description, language);
        tag.AddFrame(uslt);
        return uslt;
    }

    public static UnsynchronisedLyricsFrame? GetPreferred(Tag tag, string description, string language)
    {
        int best_value = -1;
        UnsynchronisedLyricsFrame best_frame = null;

        foreach (Frame frame in tag.GetFrames(FrameType.USLT))
        {
            if (frame is not UnsynchronisedLyricsFrame uslt)
                continue;

            bool same_name = uslt.Description == description;
            bool same_lang = uslt.Language == language;

            if (same_name && same_lang)
                return uslt;

            int value = same_lang ? 2 : same_name ? 1 : 0;

            if (value <= best_value)
                continue;

            best_value = value;
            best_frame = uslt;
        }

        return best_frame;
    }

    protected override void ParseFields(ByteVector data, byte version)
    {
        if (data.Count < 4)
            throw new CorruptFileException("Not enough bytes in field");

        TextEncoding = (StringType)data[0];
        _language = data.ToString(StringType.Latin1, 1, 3);
        string[] split = data.ToStrings(TextEncoding, 4, 2);

        if (split.Length == 1)
        {
            _description = string.Empty;
            _text = split[0];
        }
        else
        {
            _description = split[0];
            _text = split[1];
        }
    }

    protected override ByteVector RenderFields(byte version)
    {
        var encoding = CorrectEncoding(TextEncoding, version);

#pragma warning disable IDE0028 // Simplify collection initialization
        return new ByteVector
        {
            (byte)encoding,
            ByteVector.FromString (Language, StringType.Latin1),
            ByteVector.FromString (_description, encoding),
            ByteVector.TextDelimiter (encoding),
            ByteVector.FromString (_text, encoding),
        };
#pragma warning restore IDE0028 // Simplify collection initialization
    }

    public override Frame Clone() => new UnsynchronisedLyricsFrame(_description, _language, TextEncoding) { _text = _text };
}
