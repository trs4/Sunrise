namespace Sunrise.Model.TagLib.Id3v2;

public class TermsOfUseFrame : Frame
{
    private string _language;

    public TermsOfUseFrame(string language, StringType encoding)
        : base(FrameType.USER, 4)
    {
        TextEncoding = encoding;
        _language = language;
    }

    public TermsOfUseFrame(string language)
        : base(FrameType.USER, 4)
        => _language = language;

    public TermsOfUseFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal TermsOfUseFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public StringType TextEncoding { get; set; } = Tag.DefaultEncoding;

    public string Language
    {
        get => (_language != null && _language.Length > 2) ? _language.Substring(0, 3) : "XXX";
        set => _language = value;
    }

    public string Text { get; set; }

    public override string ToString() => Text;

    public static TermsOfUseFrame? Get(Tag tag, string language, bool create)
    {
        foreach (Frame f in tag.GetFrames(FrameType.USER))
        {
            if (f is TermsOfUseFrame cf && (language is null || language == cf.Language))
                return cf;
        }

        if (!create)
            return null;

        var frame = new TermsOfUseFrame(language);
        tag.AddFrame(frame);
        return frame;
    }

    public static TermsOfUseFrame? GetPreferred(Tag tag, string language)
    {
        TermsOfUseFrame? best = null;

        foreach (Frame f in tag.GetFrames(FrameType.USER))
        {
            if (f is not TermsOfUseFrame cf)
                continue;

            if (cf.Language == language)
                return cf;

#pragma warning disable CA1508 // Avoid dead conditional code
            best ??= cf;
#pragma warning restore CA1508 // Avoid dead conditional code
        }

        return best;
    }

    protected override void ParseFields(ByteVector data, byte version)
    {
        if (data.Count < 4)
            throw new CorruptFileException("Not enough bytes in field");

        TextEncoding = (StringType)data[0];
        _language = data.ToString(StringType.Latin1, 1, 3);
        Text = data.ToString(TextEncoding, 4, data.Count - 4);
    }

    protected override ByteVector RenderFields(byte version)
    {
        var encoding = CorrectEncoding(TextEncoding, version);

#pragma warning disable IDE0028 // Simplify collection initialization
        return new ByteVector
        {
            (byte)encoding,
            ByteVector.FromString (Language,
            StringType.Latin1),
            ByteVector.FromString (Text, encoding),
        };
#pragma warning restore IDE0028 // Simplify collection initialization
    }

    public override Frame Clone() => new TermsOfUseFrame(_language, TextEncoding) { Text = Text };
}
