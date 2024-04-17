namespace Sunrise.Model.TagLib.Id3v2;

public class CommentsFrame : Frame
{
    private string _language;
    private string _description;
    private string _text;

    public CommentsFrame(string description, string language, StringType encoding)
        : base(FrameType.COMM, 4)
    {
        TextEncoding = encoding;
        _language = language;
        _description = description;
    }

    public CommentsFrame(string description, string? language) : this(description, language, Tag.DefaultEncoding) { }

    public CommentsFrame(string description) : this(description, null) { }

    public CommentsFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal CommentsFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public StringType TextEncoding { get; set; } = Tag.DefaultEncoding;

    public string Language
    {
        get
        {
            if (_language is not null && _language.Length > 2)
                return _language.Substring(0, 3);

            return "XXX";
        }
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

    public static CommentsFrame? Get(Tag tag, string description, string language, bool create)
    {
        CommentsFrame comm;

        foreach (Frame frame in tag.GetFrames(FrameType.COMM))
        {
            comm = frame as CommentsFrame;

            if (comm is null)
                continue;

            if (comm.Description != description)
                continue;

            if (language is not null && language != comm.Language)
                continue;

            return comm;
        }

        if (!create)
            return null;

        comm = new CommentsFrame(description, language);
        tag.AddFrame(comm);
        return comm;
    }

    public static CommentsFrame? GetPreferred(Tag tag, string description, string language)
    {
        bool skip_itunes = description is null || !description.StartsWith("iTun");
        int best_value = -1;
        CommentsFrame best_frame = null;

        foreach (Frame frame in tag.GetFrames(FrameType.COMM))
        {
            if (frame is not CommentsFrame comm)
                continue;

            if (skip_itunes && comm.Description.StartsWith("iTun"))
                continue;

            bool same_name = comm.Description == description;
            bool same_lang = comm.Language == language;

            if (same_name && same_lang)
                return comm;

            int value = same_lang ? 2 : same_name ? 1 : 0;

            if (value <= best_value)
                continue;

            best_value = value;
            best_frame = comm;
        }

        return best_frame;
    }

    protected override void ParseFields(ByteVector data, byte version)
    {
        if (data.Count < 4)
            throw new CorruptFileException("Not enough bytes in field.");

        TextEncoding = (StringType)data[0];
        _language = data.ToString(StringType.Latin1, 1, 3);
        string[] split = data.ToStrings(TextEncoding, 4, 3);

        if (split.Length == 0)
        {
            _description = string.Empty;
            _text = string.Empty;
        }
        else if (split.Length == 1)
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
        StringType encoding = CorrectEncoding(TextEncoding, version);

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

    public override Frame Clone() => new CommentsFrame(_description, _language, TextEncoding) { _text = _text };
}
