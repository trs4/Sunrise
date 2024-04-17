namespace Sunrise.Model.TagLib.Id3v2;

public class PopularimeterFrame : Frame
{
    private string _user = string.Empty;

    public PopularimeterFrame(string user)
        : base(FrameType.POPM, 4)
        => User = user;

    public PopularimeterFrame(ByteVector data, byte version)
        : base(data, version)
        => SetData(data, 0, version, true);

    protected internal PopularimeterFrame(ByteVector data, int offset, FrameHeader header, byte version)
        : base(header)
        => SetData(data, offset, version, false);

    public string User
    {
        get => _user;
        set => _user = value ?? string.Empty;
    }

    public byte Rating { get; set; }

    public ulong PlayCount { get; set; }

    public static PopularimeterFrame? Get(Tag tag, string user, bool create)
    {
        PopularimeterFrame popm;

        foreach (Frame frame in tag)
        {
            popm = frame as PopularimeterFrame;

            if (popm != null && popm._user.Equals(user))
                return popm;
        }

        if (!create)
            return null;

        popm = new PopularimeterFrame(user);
        tag.AddFrame(popm);
        return popm;
    }

    protected override void ParseFields(ByteVector data, byte version)
    {
        var delim = ByteVector.TextDelimiter(StringType.Latin1);

        int index = data.Find(delim);

        if (index < 0)
            throw new CorruptFileException("Popularimeter frame does not contain a text delimiter");

        if (index + 2 > data.Count)
            throw new CorruptFileException("Popularimeter is too short");

        _user = data.ToString(StringType.Latin1, 0, index);
        Rating = data[index + 1];
        PlayCount = data.Mid(index + 2).ToULong();
    }

    protected override ByteVector RenderFields(byte version)
    {
        ByteVector data = ByteVector.FromULong(PlayCount);

        while (data.Count > 0 && data[0] == 0)
            data.RemoveAt(0);

        data.Insert(0, Rating);
        data.Insert(0, 0);
        data.Insert(0, ByteVector.FromString(_user, StringType.Latin1));
        return data;
    }

    public override Frame Clone() => new PopularimeterFrame(_user)
    {
        PlayCount = PlayCount,
        Rating = Rating
    };
}
